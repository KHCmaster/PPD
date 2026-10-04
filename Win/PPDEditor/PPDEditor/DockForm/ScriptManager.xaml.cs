using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using FlowScriptControl.Classes;
using PPDEditor.DockForm.Script;
using PPDEditor.Forms;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor
{
    /// <summary>
    /// スクリプトのドックです。中に自分のドッキングのレイアウト（スクリプトの一覧、タブ、ノードの一覧など）を持ちます。
    /// 編集のコマンドとデバッグは <see cref="ScriptManagerViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// スクリプトのタブ（<see cref="FlowDrawDocument"/>）とレイアウトはドッキングのコントロールの中のものなので、ここで扱う。
    /// </remarks>
    public sealed partial class ScriptManager : DockView, IScriptWorkspace, IDisposable
    {
        readonly List<FlowDrawDocument> documents = [];
        FlowDrawDocument lastActiveDocument;
        bool mainFormEventsAttached;

        public ScriptManager()
        {
            InitializeComponent();
            AttachViewModel(new ScriptManagerViewModel(this));
            logForm.DataContext = ViewModel.Log;

            scriptListForm.Renamed += ScriptListForm_Renamed;
            scriptListForm.Deleted += ScriptListForm_Deleted;
            scriptListForm.ScriptSelected += path => OpenScript(path);
            scriptListForm.Reloaded += ScriptListForm_Reloaded;

            searchPanel.Searched += query => CurrentDocument?.FlowDrawPanel.Search(query);
            searchPanel.SelectionChanged += FlowSearchPanel_SelectionChanged;

            dockingManager.ActiveContentChanged += DockingManager_ActiveContentChanged;
            // デバッグのウィンドウは、はじめは出さない
            executingPropertyAnchorable.Hide();

            FlowNodeFilters.Register();
            Loaded += ScriptManager_Loaded;
        }

        public ScriptManagerViewModel ViewModel
        {
            get { return (ScriptManagerViewModel)DataContext; }
        }

        public FlowDrawDocument[] FlowDrawForms
        {
            get { return [.. documents]; }
        }

        /// <summary>
        /// 最後に選んだスクリプトのタブです（以前の DockPanel.ActiveDocument）。
        /// </summary>
        public FlowDrawDocument CurrentDocument
        {
            get
            {
                var lastFocused = dockingManager.Layout.LastFocusedDocument;
                return documents.FirstOrDefault(d => d.Document == lastFocused)
                    ?? documents.FirstOrDefault(d => d.Document.IsSelected);
            }
        }

        IEnumerable<FlowDrawDocument> IScriptWorkspace.Documents
        {
            get { return FlowDrawForms; }
        }

        string IScriptWorkspace.ProjectDir
        {
            get { return WindowUtility.Editor.CurrentProjectDir; }
        }

        AvailableDifficulty IScriptWorkspace.CurrentDifficulty
        {
            get { return WindowUtility.Editor.CurrentDifficulty; }
        }

        void ScriptManager_Loaded(object sender, RoutedEventArgs e)
        {
            // Loaded はドックを出し直すたびに来るので、1 回だけ登録する
            if (mainFormEventsAttached)
            {
                return;
            }
            mainFormEventsAttached = true;
            WindowUtility.Editor.ProcessStarted += ViewModel.OnProcessStarted;
            WindowUtility.Editor.CommandExecuting += ViewModel.OnCommandExecuting;
            WindowUtility.Editor.ProcessExited += ViewModel.StopDebugging;
            WindowUtility.Editor.CommandExecuted += fileName => ViewModel.StopDebugging();
        }

        int? IScriptWorkspace.ShowExecutingSource(string fileName, string sourceText)
        {
            var lastSourceId = executingPropertyPanel.SourceId;
            executingPropertyPanel.ChangeSource(fileName, sourceText);
            return lastSourceId != executingPropertyPanel.SourceId ? executingPropertyPanel.SourceId : null;
        }

        void IScriptWorkspace.BringToFront()
        {
            ShowDock();
            WindowUtility.MainForm.Activate();
        }

        void IScriptWorkspace.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        void FlowSearchPanel_SelectionChanged(SearchResultItem obj)
        {
            var foundForm = FlowDrawForms.FirstOrDefault(f => f.FlowDrawPanel.Guid == obj.SearchResult.Guid);
            if (foundForm == null)
            {
                return;
            }
            if (CurrentDocument != foundForm)
            {
                foundForm.Activate();
            }

            if (obj.Source == null)
            {
                foundForm.FlowDrawPanel.SelectAndFocus(obj.Comment);
            }
            else
            {
                foundForm.FlowDrawPanel.SelectAndFocus(obj.Source);
            }
        }

        void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            // スクリプトのタブを切り替えたら、前のタブのノードのプロパティは消す
            var current = CurrentDocument;
            if (current != lastActiveDocument)
            {
                lastActiveDocument = current;
                propertyPanel.MakeEmpty();
            }
        }

        /// <summary>
        /// スクリプトのタブを開きます（開いていればそれを前に出す）。ファイルがないときは null です。
        /// </summary>
        public FlowDrawDocument OpenScript(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            var drawForm = FlowDrawForms.FirstOrDefault(f => f.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase));
            if (drawForm != null)
            {
                drawForm.Activate();
                return drawForm;
            }

            drawForm = new FlowDrawDocument();
            drawForm.Modified += DrawForm_Modified;
            drawForm.Closed += DrawForm_Closed;
            drawForm.FlowDrawPanel.SearchRequired += FlowDrawPanel_SearchRequired;
            drawForm.FlowDrawPanel.FlowPropertyPanel = propertyPanel;
            drawForm.FlowDrawPanel.FlowTreeView = treeView;
            drawForm.SetLang();
            drawForm.FilePath = filePath;
            documents.Add(drawForm);
            GetDocumentPane().Children.Add(drawForm.Document);
            drawForm.Load();
            drawForm.Activate();
            return drawForm;
        }

        /// <summary>
        /// スクリプトのタブを入れるところを返す。配置を読み込んでなくなっていたら作る。
        /// </summary>
        private LayoutDocumentPane GetDocumentPane()
        {
            var pane = dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            if (pane == null)
            {
                pane = new LayoutDocumentPane();
                dockingManager.Layout.RootPanel.Children.Add(pane);
            }
            return pane;
        }

        void DrawForm_Closed(object sender, EventArgs e)
        {
            var drawForm = (FlowDrawDocument)sender;
            documents.Remove(drawForm);
            if (lastActiveDocument == drawForm)
            {
                lastActiveDocument = null;
            }
        }

        void FlowDrawPanel_SearchRequired(string obj)
        {
            searchPanel.SearchQuery = obj;
            searchAnchorable.Show();
            searchAnchorable.IsActive = true;
        }

        /// <summary>
        /// レイアウトの中のウィンドウの名前、タブの右クリックのメニュー、タブの文字を、言語に合わせます。
        /// </summary>
        public void SetLang()
        {
            this.保存MenuItem.Header = Utility.Language["Save"];
            SetAnchorableTitles();
            foreach (FlowDrawDocument dockForm in FlowDrawForms)
            {
                dockForm.SetLang();
            }
        }

        /// <summary>
        /// ウィンドウのタイトルを言語に合わせる。配置を読み込むとタイトルも読み込まれるので、その後にも呼ぶ。
        /// </summary>
        private void SetAnchorableTitles()
        {
            if (Utility.Language == null)
            {
                return;
            }
            propertyAnchorable.Title = (string)(this.プロパティMenuItem.Header = Utility.Language["NodePropertyWindow"]);
            treeViewAnchorable.Title = (string)(this.ソースリストMenuItem.Header = Utility.Language["NodeListWindow"]);
            scriptListAnchorable.Title = (string)(this.スクリプトリストMenuItem.Header = Utility.Language["ScriptListWindow"]);
            searchAnchorable.Title = (string)(this.検索MenuItem.Header = Utility.Language["NodeSearchWindow"]);
            executingPropertyAnchorable.Title = (string)(this.デバッグMenuItem.Header = Utility.Language["Debug"]);
            logAnchorable.Title = (string)(this.ログMenuItem.Header = Utility.Language["Log"]);
        }

        // 共通の関数とコマンドのスクリプトは、プロジェクトの変更にしない
        void DrawForm_Modified(object sender, EventArgs e)
        {
            var drawForm = (FlowDrawDocument)sender;
            if (!ScriptPaths.IsUnderFunctions(drawForm.FilePath) && !ScriptPaths.IsUnderCommands(drawForm.FilePath))
            {
                ContentChanged();
            }
        }

        void ScriptListForm_Reloaded()
        {
            foreach (FlowDrawDocument drawForm in FlowDrawForms.Where(d => !File.Exists(d.FilePath)))
            {
                drawForm.Close();
            }
        }

        void ScriptListForm_Renamed(string oldPath, string newPath)
        {
            if (Path.GetExtension(oldPath) != "")
            {
                // ファイルの名前を変えた
                foreach (FlowDrawDocument drawForm in FlowDrawForms)
                {
                    if (drawForm.FilePath.Equals(oldPath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        drawForm.FilePath = newPath;
                    }
                }
            }
            else
            {
                // フォルダーの名前を変えた
                foreach (FlowDrawDocument drawForm in FlowDrawForms)
                {
                    if (ScriptPaths.IsChildFolderOrFile(oldPath, drawForm.FilePath))
                    {
                        drawForm.FilePath = Path.Combine(newPath, drawForm.FilePath[(oldPath.Length + 1)..]);
                    }
                }
            }
        }

        void ScriptListForm_Deleted(string path)
        {
            foreach (FlowDrawDocument drawForm in FlowDrawForms.Where(d => d.FilePath.Equals(path, StringComparison.CurrentCultureIgnoreCase) || ScriptPaths.IsChildFolderOrFile(path, d.FilePath)).ToArray())
            {
                drawForm.Close();
            }
        }

        /// <summary>
        /// ウィンドウの配置を読み込みます。以前の WinForms 版の形式など、読み込めないときは既定の配置のままにします。
        /// </summary>
        public void RestoreDock(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var serializer = new XmlLayoutSerializer(dockingManager);
                    serializer.LayoutSerializationCallback += (sender, e) =>
                    {
                        // スクリプトのタブは配置に入れない
                        e.Content = e.Model.ContentId switch
                        {
                            "PPDEditor.DockForm.Script.FlowPropertyDockForm" => propertyPanel,
                            "PPDEditor.DockForm.Script.FlowTreeViewDockForm" => treeView,
                            "PPDEditor.DockForm.Script.ScriptListDockForm" => scriptListForm,
                            "PPDEditor.DockForm.Script.FlowSearchDockForm" => searchPanel,
                            "PPDEditor.DockForm.Script.FlowExecutingPropertyDockForm" => executingPropertyPanel,
                            "PPDEditor.DockForm.Script.FlowLogDockForm" => logForm,
                            _ => null,
                        };
                        e.Cancel = e.Content == null;
                    };
                    serializer.Deserialize(filePath);
                }
                catch (Exception)
                {
                }
            }
            RebindAnchorables();
            SetAnchorableTitles();
        }

        /// <summary>
        /// 配置を読み込むと、ウィンドウ（LayoutAnchorable）は新しく作られるので、「表示」メニューで扱うものを付け替えます。
        /// 読み込んだ配置になかったウィンドウは、配置に加え直します。
        /// </summary>
        private void RebindAnchorables()
        {
            var anchorables = dockingManager.Layout.Descendents().OfType<LayoutAnchorable>()
                .Concat(dockingManager.Layout.Hidden)
                .Distinct()
                .ToList();
            propertyAnchorable = Rebind(anchorables, propertyAnchorable);
            treeViewAnchorable = Rebind(anchorables, treeViewAnchorable);
            scriptListAnchorable = Rebind(anchorables, scriptListAnchorable);
            searchAnchorable = Rebind(anchorables, searchAnchorable);
            executingPropertyAnchorable = Rebind(anchorables, executingPropertyAnchorable);
            logAnchorable = Rebind(anchorables, logAnchorable);
        }

        private LayoutAnchorable Rebind(List<LayoutAnchorable> anchorables, LayoutAnchorable original)
        {
            var found = anchorables.FirstOrDefault(a => a.ContentId == original.ContentId);
            if (found != null)
            {
                return found;
            }
            if (original.Root == null)
            {
                original.AddToLayout(dockingManager, AnchorableShowStrategy.Most);
            }
            return original;
        }

        public void SaveDock(string filePath)
        {
            try
            {
                new XmlLayoutSerializer(dockingManager).Serialize(filePath);
            }
            catch (IOException)
            {
            }
        }

        public void Clear()
        {
            scriptListForm.Clear();
            foreach (FlowDrawDocument drawForm in FlowDrawForms.Where(d => !ScriptPaths.IsUnderFunctions(d.FilePath)))
            {
                drawForm.Close();
            }
        }

        public void SaveAll()
        {
            foreach (FlowDrawDocument drawForm in FlowDrawForms)
            {
                if (drawForm.IsContentChanged && !ScriptPaths.IsUnderFunctions(drawForm.FilePath) && !ScriptPaths.IsUnderCommands(drawForm.FilePath))
                {
                    drawForm.Save();
                }
            }
        }

        public void ReadScript(string projectDirPath, AvailableDifficulty difficulty, string disableList)
        {
            scriptListForm.ReadScript(projectDirPath, difficulty, disableList);
        }

        public string GetDisableScriptList()
        {
            return scriptListForm.GetDisabledScriptList();
        }

        public string[] GetEnabledScriptList()
        {
            return scriptListForm.GetEnabledScriptList();
        }

        public void WriteScript(Stream stream, string scriptName)
        {
            scriptListForm.WriteScript(stream, scriptName);
        }

        private void 表示MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            this.スクリプトリストMenuItem.IsChecked = DockUtility.IsShown(scriptListAnchorable);
            this.ソースリストMenuItem.IsChecked = DockUtility.IsShown(treeViewAnchorable);
            this.プロパティMenuItem.IsChecked = DockUtility.IsShown(propertyAnchorable);
            this.検索MenuItem.IsChecked = DockUtility.IsShown(searchAnchorable);
            this.デバッグMenuItem.IsChecked = DockUtility.IsShown(executingPropertyAnchorable);
            this.ログMenuItem.IsChecked = DockUtility.IsShown(logAnchorable);
        }

        private void プロパティMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockUtility.ShowOrHideWindow(propertyAnchorable);
        }

        private void スクリプトリストMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockUtility.ShowOrHideWindow(scriptListAnchorable);
        }

        private void ソースリストMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockUtility.ShowOrHideWindow(treeViewAnchorable);
        }

        private void 検索MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockUtility.ShowOrHideWindow(searchAnchorable);
        }

        private void デバッグMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockUtility.ShowOrHideWindow(executingPropertyAnchorable);
        }

        private void ログMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockUtility.ShowOrHideWindow(logAnchorable);
        }

        /// <summary>
        /// タブの右クリックのメニューで選んだスクリプトのタブを返す。
        /// </summary>
        private FlowDrawDocument GetContextMenuDocument(object sender)
        {
            var menuItem = (MenuItem)sender;
            var model = (menuItem.DataContext as LayoutDocumentItem)?.Model;
            return documents.FirstOrDefault(d => d.Document == model);
        }

        private void 保存MenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetContextMenuDocument(sender)?.Save();
        }

        private void 閉じるMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetContextMenuDocument(sender)?.Close();
        }

        /// <summary>
        /// 編集のメニューのショートカットキーです。ノードの図にフォーカスがあるときだけ処理し、入力欄などのキーはそのままにする。
        /// </summary>
        private void ScriptManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentDocument == null || !CurrentDocument.FlowDrawPanel.IsControlFocused)
            {
                return;
            }
            ICommand command = (Keyboard.Modifiers, e.Key) switch
            {
                (ModifierKeys.Control, Key.Z) => ViewModel.UndoCommand,
                (ModifierKeys.Control, Key.Y) => ViewModel.RedoCommand,
                (ModifierKeys.Control, Key.X) => ViewModel.CutCommand,
                (ModifierKeys.Control, Key.C) => ViewModel.CopyCommand,
                (ModifierKeys.Control, Key.V) => ViewModel.PasteCommand,
                (ModifierKeys.Control | ModifierKeys.Shift, Key.V) => ViewModel.PasteWithLinksCommand,
                (ModifierKeys.None, Key.Delete) => ViewModel.DeleteCommand,
                _ => null,
            };
            if (command != null)
            {
                command.Execute(null);
                e.Handled = true;
            }
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }
    }
}
