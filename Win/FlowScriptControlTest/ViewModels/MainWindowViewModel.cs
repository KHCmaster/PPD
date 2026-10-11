using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowScriptControl.Classes;
using FlowScriptControl.Controls;
using FlowScriptControlTest.Models;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FlowScriptControlTest.ViewModels
{
    /// <summary>
    /// ViewModel から使う、ウィンドウの機能です。
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// 編集の領域を作ります（ノードの一覧やプロパティの表示とつないだもの）。
        /// </summary>
        FlowDrawPanel CreateEditor();

        /// <summary>
        /// 検索のタブを表示して、検索します。
        /// </summary>
        void ShowSearch(string query);

        /// <summary>
        /// プロパティの表示を空にします。
        /// </summary>
        void ClearProperty();

        /// <summary>
        /// デバッグのタブに、実行しているノードを表示します。
        /// </summary>
        /// <returns>表示するノードが変わったときは、そのノードの ID。変わらないときは null。</returns>
        int? ShowExecutingSource(string filePath, string sourceText);

        /// <summary>
        /// 開くファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskOpenFile();

        /// <summary>
        /// 保存するファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskSaveFile();

        /// <summary>
        /// 標準入力の文字列を編集させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskStdin(string text);

        void ShowMessage(string text);

        bool Confirm(string text, string caption);
    }

    /// <summary>
    /// フロースクリプトを編集して実行・デバッグする画面です。
    /// </summary>
    /// <remarks>
    /// スクリプトは別のスレッドで動くので、実行の通知は UI のスレッドに移してから処理する。
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "consoleWriter はスクリプトの実行が終わったときと、ウィンドウを閉じたとき（Shutdown）に破棄する")]
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// ショートカットキーから実行したことを表すコマンドの引数です。
        /// </summary>
        public const string FromShortcut = "Shortcut";

        readonly IWindowService window;
        readonly StringBuilder console = new();
        SynchronizationContext context;
        Executor currentExecutor;
        ConsoleWriter consoleWriter;
        string stdinText = "";
        bool isShutdown;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UndoCommand), nameof(RedoCommand), nameof(CopyCommand), nameof(PasteCommand), nameof(PasteWithLinksCommand),
            nameof(DeleteCommand), nameof(AdjustPositionCommand), nameof(ExpandAllCommand), nameof(CollapseAllCommand), nameof(OpenCommand), nameof(SaveCommand))]
        public partial FlowDocumentViewModel SelectedDocument { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand), nameof(StopCommand))]
        public partial bool IsExecuting { get; set; }

        /// <summary>
        /// ブレークポイントなどで止まっているかです（続行・ステップインができる）。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand), nameof(StepInCommand))]
        public partial bool IsWaiting { get; set; }

        public MainWindowViewModel(IWindowService window)
        {
            this.window = window;
            NewDocument();
        }

        public ObservableCollection<FlowDocumentViewModel> Documents { get; } = [];

        public string ConsoleText
        {
            get { return console.ToString(); }
        }

        partial void OnSelectedDocumentChanged(FlowDocumentViewModel value)
        {
            window.ClearProperty();
        }

        private bool HasDocument()
        {
            return SelectedDocument != null;
        }

        // ショートカットキーのときは、編集の領域にフォーカスがあるときだけ使う（テキストボックスなどでは、そちらの操作になる）
        private FlowDrawPanel GetEditor(string source)
        {
            var editor = SelectedDocument?.Editor;
            if (editor == null || (source == FromShortcut && !editor.IsControlFocused))
            {
                return null;
            }
            return editor;
        }

        [RelayCommand]
        private void NewDocument()
        {
            var editor = window.CreateEditor();
            var document = new FlowDocumentViewModel(editor);
            editor.SearchRequired += window.ShowSearch;
            Documents.Add(document);
            SelectedDocument = document;
        }

        [RelayCommand]
        private void CloseDocument(FlowDocumentViewModel document)
        {
            Documents.Remove(document);
            document.Editor.Dispose();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Open()
        {
            var fileName = window.AskOpenFile();
            if (fileName == null)
            {
                return;
            }
            using (var stream = File.OpenRead(fileName))
            {
                SelectedDocument.Editor.LoadXML(stream);
            }
            SelectedDocument.FilePath = fileName;
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Save()
        {
            var fileName = window.AskSaveFile();
            if (fileName != null)
            {
                using var stream = File.Create(fileName);
                SelectedDocument.Editor.SaveXML(stream);
            }
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Undo(string source)
        {
            GetEditor(source)?.Undo();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Redo(string source)
        {
            GetEditor(source)?.Redo();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Copy(string source)
        {
            GetEditor(source)?.Copy();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Paste(string source)
        {
            GetEditor(source)?.PasteAtCursor();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void PasteWithLinks(string source)
        {
            GetEditor(source)?.PasteWithLinksAtCursor();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void Delete(string source)
        {
            GetEditor(source)?.Delete();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void AdjustPosition()
        {
            SelectedDocument.Editor.AdjustPosition();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void ExpandAll()
        {
            SelectedDocument.Editor.ExpandAll();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private void CollapseAll()
        {
            SelectedDocument.Editor.CollapseAll();
        }

        [RelayCommand]
        private void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // コンソールがない（リダイレクトされている）ときは消せない
            }
            console.Clear();
            OnPropertyChanged(nameof(ConsoleText));
        }

        [RelayCommand]
        private void EditStdin()
        {
            stdinText = window.AskStdin(stdinText) ?? stdinText;
        }

        /// <summary>
        /// 検索のパネルから、今のスクリプトを検索します。
        /// </summary>
        public SearchResult Search(string query)
        {
            return SelectedDocument?.Editor.Search(query);
        }

        /// <summary>
        /// 検索の結果で選んだノード（またはコメント）を表示します。
        /// </summary>
        public void ShowSearchResult(SearchResultItem item)
        {
            var document = Documents.FirstOrDefault(d => d.Editor.Guid == item.SearchResult.Guid);
            if (document == null)
            {
                return;
            }
            SelectedDocument = document;
            if (item.Source == null)
            {
                document.Editor.SelectAndFocus(item.Comment);
            }
            else
            {
                document.Editor.SelectAndFocus(item.Source);
            }
        }

        private bool CanExecute()
        {
            return !IsExecuting && Documents.Count > 0;
        }

        // すべてのタブのスクリプトを実行する
        [RelayCommand(CanExecute = nameof(CanExecute))]
        private void Execute()
        {
            var executeInfos = Documents.Select(document =>
            {
                var stream = new MemoryStream();
                document.Editor.SaveXML(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new ExecuteInfo(stream, document.Editor.GetBreakPoints(), document, document.Editor.GetSources());
            }).ToArray();
            context = SynchronizationContext.Current;
            consoleWriter = new ConsoleWriter();
            consoleWriter.Output += text => RunOnUIThread(() => AppendConsole(text));
            currentExecutor = new Executor(executeInfos, stdinText);
            currentExecutor.Finished += () => RunOnUIThread(Executor_Finished);
            currentExecutor.Aborted += () => RunOnUIThread(Executor_Aborted);
            currentExecutor.Controller.SourceChanged += (text, manager) => RunOnUIThread(() => Controller_SourceChanged(text, manager));
            currentExecutor.Controller.OperationAccepted += manager => RunOnUIThread(() => Controller_OperationAccepted(manager));
            currentExecutor.Controller.OperationWaited += manager => RunOnUIThread(() => Controller_OperationWaited(manager));
            currentExecutor.Controller.ErrorOccurred += (manager, exception) => RunOnUIThread(() => Controller_ErrorOccurred(manager, exception));
            IsWaiting = false;
            IsExecuting = true;
            currentExecutor.Execute();
        }

        [RelayCommand(CanExecute = nameof(IsWaiting))]
        private void Continue()
        {
            currentExecutor.Controller.Continue();
        }

        [RelayCommand(CanExecute = nameof(IsExecuting))]
        private void Stop()
        {
            currentExecutor.Abort();
        }

        [RelayCommand(CanExecute = nameof(IsWaiting))]
        private void StepIn()
        {
            currentExecutor.Controller.StepIn();
        }

        // 閉じた後（スクリプトを止めている間も含む）の通知は捨てる
        private void RunOnUIThread(Action action)
        {
            if (isShutdown)
            {
                return;
            }
            context.Send(_ =>
            {
                if (!isShutdown)
                {
                    action();
                }
            }, null);
        }

        private void AppendConsole(string text)
        {
            console.Append(text);
            OnPropertyChanged(nameof(ConsoleText));
        }

        private void Controller_ErrorOccurred(FlowSourceManager manager, FlowExecutionException exception)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(manager);
            if (executeInfo == null)
            {
                return;
            }
            var errorText = exception.ToString();
            if (exception.SourceObject != null)
            {
                executeInfo.Document.Editor.ShowError(exception.SourceObject.Id, errorText);
            }
            else
            {
                window.ShowMessage(errorText);
            }
        }

        // 止まっている間は、選んだノードの値をデバッグのタブで見られるようにする
        private void Controller_OperationWaited(FlowSourceManager manager)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(manager);
            executeInfo?.Document.Editor.SelectionChanged += Editor_SelectionChanged;
            IsWaiting = true;
        }

        private void Controller_OperationAccepted(FlowSourceManager manager)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(manager);
            executeInfo?.Document.Editor.SelectionChanged -= Editor_SelectionChanged;
            IsWaiting = false;
        }

        private void Controller_SourceChanged(string sourceText, FlowSourceManager manager)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(manager);
            if (executeInfo == null)
            {
                return;
            }
            SelectedDocument = executeInfo.Document;
            var sourceId = window.ShowExecutingSource(executeInfo.Document.FilePath, sourceText);
            if (sourceId.HasValue)
            {
                executeInfo.Document.Editor.SelectAndFocus(sourceId.Value);
            }
        }

        private void Editor_SelectionChanged(object sender, EventArgs e)
        {
            var ids = ((FlowDrawPanel)sender).GetSelectedSourceId();
            if (ids.Length == 1)
            {
                currentExecutor.Controller.ChangeSource(ids[0]);
            }
        }

        private void Executor_Finished()
        {
            IsExecuting = false;
            IsWaiting = false;
            consoleWriter?.Close();
            consoleWriter = null;
        }

        private void Executor_Aborted()
        {
            window.ShowMessage("スクリプトのプロセスが終了しなかったためKillしました。");
            consoleWriter?.Stop();
            consoleWriter = null;
        }

        /// <summary>
        /// ウィンドウを閉じてよいかを確かめ、よければ実行を止めて後片付けをします。
        /// </summary>
        public bool TryShutdown()
        {
            if (currentExecutor != null && currentExecutor.IsExecuting)
            {
                if (!window.Confirm("実行中のスクリプトがあります。終了しますか？", "確認"))
                {
                    return false;
                }
                // 止めたときの通知でメッセージなどを出さないように、通知を受け取らないようにしてから止める
                isShutdown = true;
                currentExecutor.Abort();
            }
            foreach (var document in Documents)
            {
                document.Editor.Dispose();
            }
            consoleWriter?.Stop();
            consoleWriter?.Dispose();
            consoleWriter = null;
            return true;
        }
    }
}
