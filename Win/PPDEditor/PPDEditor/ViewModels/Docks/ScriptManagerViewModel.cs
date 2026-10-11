using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowScriptControl.Controls;
using FlowScriptDrawControl.Model;
using PPDCore;
using PPDEditor.DockForm.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// スクリプトのタブを扱う、画面の機能です（タブはドッキングのレイアウトの中にあるので、画面が持つ）。
    /// </summary>
    public interface IScriptWorkspace
    {
        /// <summary>
        /// 最後に選んだスクリプトのタブです。
        /// </summary>
        FlowDrawDocument CurrentDocument { get; }

        IEnumerable<FlowDrawDocument> Documents { get; }

        /// <summary>
        /// スクリプトのタブを開きます（開いていればそれを前に出す）。ファイルがないときは null です。
        /// </summary>
        FlowDrawDocument OpenScript(string filePath);

        string ProjectDir { get; }

        AvailableDifficulty CurrentDifficulty { get; }

        /// <summary>
        /// 有効なスクリプトのパスです（プロジェクトの中からの相対パス）。
        /// </summary>
        string[] GetEnabledScriptList();

        /// <summary>
        /// デバッグのウィンドウに、実行しているノードを表示します。
        /// </summary>
        /// <returns>表示するノードが変わったときは、そのノードの ID。変わらないときは null。</returns>
        int? ShowExecutingSource(string fileName, string sourceText);

        /// <summary>
        /// スクリプトのドックとメインのウィンドウを前に出します。
        /// </summary>
        void BringToFront();

        void ShowMessage(string text);
    }

    /// <summary>
    /// スクリプトのドックです。編集のコマンドと、ゲームで実行したスクリプトのデバッグ（止める、続ける、1 つずつ進める）を扱います。
    /// </summary>
    /// <remarks>
    /// デバッグの通知は別のスレッドから来るので、UI のスレッドに移してから処理する。
    /// </remarks>
    public partial class ScriptManagerViewModel(IScriptWorkspace workspace) : DockViewModel, IDisposable
    {
        readonly IScriptWorkspace workspace = workspace;
        readonly SynchronizationContext context = SynchronizationContext.Current ?? new SynchronizationContext();
        TcpDebugControllerHost controllerHost;
        Source[] currentSources;
        string currentFileName;

        /// <summary>
        /// デバッグしているかです（デバッグのツールバーを出す）。
        /// </summary>
        [ObservableProperty]
        private bool isDebugging;

        /// <summary>
        /// スクリプトが止まっているかです（続行・ステップインができる）。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand), nameof(StepInCommand))]
        private bool isWaiting;

        public FlowLogViewModel Log { get; } = new();

        private void RunOnUIThread(Action action)
        {
            context.Send(_ => action(), null);
        }

        // メニューでもショートカットキーでも、ノードの図にフォーカスがあるときだけ使う（以前と同じ。入力欄などのキーはそのままにする）
        private FlowDrawPanel GetFocusedPanel()
        {
            var panel = workspace.CurrentDocument?.FlowDrawPanel;
            return panel != null && panel.IsControlFocused ? panel : null;
        }

        [RelayCommand]
        private void Undo() => GetFocusedPanel()?.Undo();

        [RelayCommand]
        private void Redo() => GetFocusedPanel()?.Redo();

        [RelayCommand]
        private void Cut() => GetFocusedPanel()?.Cut();

        [RelayCommand]
        private void Copy() => GetFocusedPanel()?.Copy();

        [RelayCommand]
        private void Paste() => GetFocusedPanel()?.PasteAtCursor();

        [RelayCommand]
        private void PasteWithLinks() => GetFocusedPanel()?.PasteWithLinksAtCursor();

        [RelayCommand]
        private void Delete() => GetFocusedPanel()?.Delete();

        [RelayCommand]
        private void FitView() => workspace.CurrentDocument?.FlowDrawPanel.FitToView();

        [RelayCommand]
        private void ExpandAll() => workspace.CurrentDocument?.FlowDrawPanel.ExpandAll();

        [RelayCommand]
        private void CollapseAll() => workspace.CurrentDocument?.FlowDrawPanel.CollapseAll();

        /// <summary>
        /// ゲームを始めたときに、有効なスクリプトをデバッグできるようにします（開いているタブのブレークポイントを渡す）。
        /// </summary>
        public void OnProcessStarted()
        {
            var infos = new Dictionary<string, int[]>();
            foreach (var scriptFilePath in workspace.GetEnabledScriptList())
            {
                var document = workspace.Documents.FirstOrDefault(d => !ScriptPaths.IsUnderFunctions(d.FilePath)
                    && d.FilePath.ToLower().Contains(scriptFilePath.ToLower(), StringComparison.CurrentCulture));
                infos.Add(scriptFilePath, document?.FlowDrawPanel.GetBreakPoints() ?? []);
            }
            StartDebugging(infos);
        }

        /// <summary>
        /// コマンドのスクリプトを実行するときに、そのスクリプトをデバッグできるようにします。
        /// </summary>
        public void OnCommandExecuting(string fileName)
        {
            var document = FindDocument(fileName);
            if (document != null)
            {
                StartDebugging(new Dictionary<string, int[]> { { fileName, document.FlowDrawPanel.GetBreakPoints() } });
            }
        }

        /// <summary>
        /// ゲームやコマンドが終わったときに、デバッグをやめます。
        /// </summary>
        public void StopDebugging()
        {
            IsDebugging = false;
            IsWaiting = false;
            controllerHost?.Dispose();
            foreach (var document in workspace.Documents)
            {
                document.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
                currentSources = document.FlowDrawPanel.GetSources();
                foreach (var source in currentSources)
                {
                    source.PropertyChanged -= Source_PropertyChanged;
                }
            }
        }

        private void StartDebugging(Dictionary<string, int[]> infos)
        {
            IsDebugging = true;
            IsWaiting = false;
            controllerHost = new TcpDebugControllerHost(infos);
            controllerHost.SourceChanged += (sourceText, fileName) => RunOnUIThread(() => ControllerHost_SourceChanged(sourceText, fileName));
            controllerHost.OperationWaited += fileName => RunOnUIThread(() => ControllerHost_OperationWaited(fileName));
            controllerHost.OperationAccepted += fileName => RunOnUIThread(() => ControllerHost_OperationAccepted(fileName));
            controllerHost.ErrorOccurred += (fileName, sourceId, text) => RunOnUIThread(() => ControllerHost_ErrorOccurred(fileName, sourceId, text));
            controllerHost.LogReceived += (logType, text) => RunOnUIThread(() => Log.AddLog(logType, text));
            controllerHost.Create();
        }

        [RelayCommand(CanExecute = nameof(IsWaiting))]
        private void Continue()
        {
            controllerHost.Continue();
            IsWaiting = false;
        }

        [RelayCommand(CanExecute = nameof(IsWaiting))]
        private void StepIn()
        {
            controllerHost.StepIn();
            IsWaiting = false;
        }

        private FlowDrawDocument FindDocument(string fileName)
        {
            return workspace.Documents.FirstOrDefault(d => d.FilePath.ToLower().Contains(fileName.ToLower(), StringComparison.CurrentCulture));
        }

        // 開いていないスクリプトは、今の難易度のスクリプトとして開く
        private FlowDrawDocument FindOrOpenDocument(string fileName)
        {
            var document = FindDocument(fileName);
            if (document == null)
            {
                var filePath = Path.Combine(workspace.ProjectDir, String.Format("{0}_{1}", workspace.CurrentDifficulty, fileName));
                document = workspace.OpenScript(Path.ChangeExtension(filePath, ".fsml"));
            }
            return document;
        }

        private void ControllerHost_ErrorOccurred(string fileName, int sourceId, string text)
        {
            var document = FindOrOpenDocument(fileName);
            if (document != null)
            {
                document.FlowDrawPanel.ShowError(sourceId, text);
            }
            else
            {
                workspace.ShowMessage(text);
            }
        }

        private void ControllerHost_OperationAccepted(string fileName)
        {
            var document = FindDocument(fileName);
            document?.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
            if (currentSources != null)
            {
                foreach (var source in currentSources)
                {
                    source.PropertyChanged -= Source_PropertyChanged;
                }
                currentSources = null;
            }
            IsWaiting = false;
        }

        // 止まっている間は、選んだノードの値を見たり、ブレークポイントを変えたりできるようにする
        private void ControllerHost_OperationWaited(string fileName)
        {
            currentFileName = fileName;
            var document = FindOrOpenDocument(fileName);
            if (document != null)
            {
                document.FlowDrawPanel.SelectionChanged += FlowDrawPanel_SelectionChanged;
                currentSources = document.FlowDrawPanel.GetSources();
                foreach (var source in currentSources)
                {
                    source.PropertyChanged += Source_PropertyChanged;
                }
            }
            IsWaiting = true;
        }

        private void ControllerHost_SourceChanged(string sourceText, string fileName)
        {
            var document = FindOrOpenDocument(fileName);
            if (document != null)
            {
                document.Activate();
                var sourceId = workspace.ShowExecutingSource(fileName, sourceText);
                if (sourceId.HasValue)
                {
                    document.FlowDrawPanel.SelectAndFocus(sourceId.Value);
                }
            }
            workspace.BringToFront();
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsBreakPointSet")
            {
                return;
            }
            var source = (Source)sender;
            if (source.IsBreakPointSet)
            {
                controllerHost.AddBreakPoint(currentFileName, source.Id);
            }
            else
            {
                controllerHost.RemoveBreakPoint(currentFileName, source.Id);
            }
        }

        private void FlowDrawPanel_SelectionChanged(object sender, EventArgs e)
        {
            var ids = ((FlowDrawPanel)sender).GetSelectedSourceId();
            if (ids.Length == 1)
            {
                controllerHost.ChangeSource(currentFileName, ids[0]);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                controllerHost?.Dispose();
                controllerHost = null;
            }
        }
    }

    /// <summary>
    /// スクリプトを実行したときのログです。
    /// </summary>
    public partial class FlowLogViewModel : ObservableObject
    {
        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        public ObservableCollection<KeyValuePair<string, string>> Logs { get; } = [];

        public void AddLog(string logType, string text)
        {
            Logs.Add(new(logType, text));
        }

        [RelayCommand]
        private void Clear()
        {
            Logs.Clear();
        }
    }

    /// <summary>
    /// スクリプトのファイルの場所を調べます。
    /// </summary>
    public static class ScriptPaths
    {
        /// <summary>
        /// エディターの Functions フォルダー（共通の関数）の中かです。
        /// </summary>
        public static bool IsUnderFunctions(string filePath)
        {
            return IsChildFolderOrFile(Path.Combine(Utility.AppDir, "Functions"), filePath);
        }

        /// <summary>
        /// エディターの Commands フォルダー（コマンド）の中かです。
        /// </summary>
        public static bool IsUnderCommands(string filePath)
        {
            return IsChildFolderOrFile(Path.Combine(Utility.AppDir, "Commands"), filePath);
        }

        public static bool IsChildFolderOrFile(string parent, string child)
        {
            return child.Contains(parent, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
