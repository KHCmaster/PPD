using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using FlowScriptControl.Classes;
using FlowScriptControl.Controls;
using FlowScriptDrawControl.Model;
using PPDCore;
using PPDEditor.DockForm.Script;
using PPDEditor.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor
{
    public partial class ScriptManager : DockView, IDisposable
    {
        readonly List<FlowDrawDocument> documents = [];
        private TcpDebugControllerHost controllerHost;
        private Source[] currentSources;
        private string currentFileName;
        private bool mainFormEventsAttached;

        public FlowDrawDocument[] FlowDrawForms
        {
            get
            {
                return [.. documents];
            }
        }

        /// <summary>
        /// 最後に選んだスクリプトのタブです（以前の DockPanel.ActiveDocument）。
        /// </summary>
        public FlowDrawDocument CurrentFlowDrawForm
        {
            get
            {
                var lastFocused = dockingManager.Layout.LastFocusedDocument;
                return documents.FirstOrDefault(d => d.Document == lastFocused)
                    ?? documents.FirstOrDefault(d => d.Document.IsSelected);
            }
        }

        public ScriptManager()
        {
            InitializeComponent();

            scriptListForm.Renamed += ScriptListForm_Renamed;
            scriptListForm.Deleted += ScriptListForm_Deleted;
            scriptListForm.ScriptSelected += ScriptListForm_ScriptSelected;
            scriptListForm.Reloaded += ScriptListForm_Reloaded;

            searchPanel.Searched += FlowSearchPanel_Searched;
            searchPanel.SelectionChanged += FlowSearchPanel_SelectionChanged;

            dockingManager.ActiveContentChanged += DockingManager_ActiveContentChanged;
            // デバッグのウィンドウは、はじめは出さない
            executingPropertyAnchorable.Hide();

            FlowDrawPanel.EnumerateClasses("dlls", ["FlowScriptEngineConsole"]);
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Script)", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null, null, [PPDModRegex()]));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Mod)", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPDEditor", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPDEditor"),
                NamespaceFilter("FlowScriptEngineData")],
                null));

            Loaded += ScriptManager_Loaded;
        }

        void ScriptManager_Loaded(object sender, RoutedEventArgs e)
        {
            // Loaded はドックを出し直すたびに来るので、1 回だけ登録する
            if (mainFormEventsAttached)
            {
                return;
            }
            mainFormEventsAttached = true;
            WindowUtility.MainForm.ProcessStarted += MainForm_ProcessStarted;
            WindowUtility.MainForm.CommandExecuting += MainForm_CommandExecuting;
            WindowUtility.MainForm.ProcessExited += MainForm_ProcessExited;
            WindowUtility.MainForm.CommandExecuted += MainForm_CommandExecuted;
        }

        void MainForm_ProcessExited()
        {
            DisableControllerHost();
        }

        void MainForm_ProcessStarted()
        {
            var enabledScriptList = GetEnabledScriptList();
            var infos = new Dictionary<string, int[]>();
            foreach (var scriptFilePath in enabledScriptList)
            {
                int[] breakPoints = null;
                foreach (var flowDrawForm in FlowDrawForms)
                {
                    var filePath = flowDrawForm.FilePath.ToLower();
                    if (!IsUnderFunctions(flowDrawForm.FilePath) && filePath.Contains(scriptFilePath.ToLower(), StringComparison.CurrentCulture))
                    {
                        breakPoints = flowDrawForm.FlowDrawPanel.GetBreakPoints();
                        break;
                    }
                }
                breakPoints ??= [];
                infos.Add(scriptFilePath, breakPoints);
            }
            EnableControllerHost(infos);
        }

        private void EnableControllerHost(Dictionary<string, int[]> infos)
        {
            toolStrip1.Visibility = Visibility.Visible;
            toolStripButton1.IsEnabled = toolStripButton2.IsEnabled = false;
            controllerHost = new TcpDebugControllerHost(infos);
            controllerHost.SourceChanged += ControllerHost_SourceChanged;
            controllerHost.OperationWaited += ControllerHost_OperationWaited;
            controllerHost.OperationAccepted += ControllerHost_OperationAccepted;
            controllerHost.ErrorOccurred += ControllerHost_ErrorOccurred;
            controllerHost.LogReceived += ControllerHost_LogReceived;
            controllerHost.Create();
        }

        private void DisableControllerHost()
        {
            toolStrip1.Visibility = Visibility.Collapsed;
            controllerHost?.Dispose();
            foreach (var flowDrawDockForm in FlowDrawForms)
            {
                flowDrawDockForm.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
                currentSources = flowDrawDockForm.FlowDrawPanel.GetSources();
                foreach (var source in currentSources)
                {
                    source.PropertyChanged -= Source_PropertyChanged;
                }
            }
        }

        void MainForm_CommandExecuting(string arg)
        {
            var dockForm = GetFlowDrawDockForm(arg);
            if (dockForm == null)
            {
                return;
            }
            var infos = new Dictionary<string, int[]>
            {
                { arg, dockForm.FlowDrawPanel.GetBreakPoints() }
            };
            EnableControllerHost(infos);
        }

        void MainForm_CommandExecuted(string obj)
        {
            DisableControllerHost();
        }

        private FlowDrawDocument GetFlowDrawDockForm(string fileName)
        {
            return FlowDrawForms.FirstOrDefault(f => f.FilePath.ToLower().Contains(fileName.ToLower(), StringComparison.CurrentCulture));
        }

        private void ToolStripButton1_Click(object sender, RoutedEventArgs e)
        {
            controllerHost.Continue();
            toolStripButton1.IsEnabled = toolStripButton2.IsEnabled = false;
        }

        private void ToolStripButton2_Click(object sender, RoutedEventArgs e)
        {
            controllerHost.StepIn();
            toolStripButton1.IsEnabled = toolStripButton2.IsEnabled = false;
        }

        private void ControllerHost_LogReceived(string arg1, string arg2)
        {
            Dispatcher.Invoke(() =>
            {
                logForm.AddLog(arg1, arg2);
            });
        }

        void ControllerHost_ErrorOccurred(string arg1, int arg2, string arg3)
        {
            Dispatcher.Invoke(() =>
            {
                var flowDrawDockForm = GetFlowDrawDockForm(arg1);
                if (flowDrawDockForm == null)
                {
                    var filePath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}", WindowUtility.MainForm.CurrentDifficulty, arg1));
                    filePath = Path.ChangeExtension(filePath, ".fsml");
                    flowDrawDockForm = AddFlowDrawForm(filePath);
                }
                if (flowDrawDockForm != null)
                {
                    flowDrawDockForm.FlowDrawPanel.ShowError(arg2, arg3);
                }
                else
                {
                    MessageBox.Show(Window.GetWindow(this), arg3);
                }
            });
        }

        void ControllerHost_OperationAccepted(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                var flowDrawDockForm = GetFlowDrawDockForm(obj);
                flowDrawDockForm?.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
                if (currentSources != null)
                {
                    foreach (var source in currentSources)
                    {
                        source.PropertyChanged -= Source_PropertyChanged;
                    }
                    currentSources = null;
                }
                toolStripButton1.IsEnabled = toolStripButton2.IsEnabled = false;
            });
        }

        void ControllerHost_OperationWaited(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                currentFileName = obj;
                var flowDrawDockForm = GetFlowDrawDockForm(obj);
                if (flowDrawDockForm == null)
                {
                    var filePath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}", WindowUtility.MainForm.CurrentDifficulty, obj));
                    filePath = Path.ChangeExtension(filePath, ".fsml");
                    flowDrawDockForm = AddFlowDrawForm(filePath);
                }
                if (flowDrawDockForm != null)
                {
                    flowDrawDockForm.FlowDrawPanel.SelectionChanged += FlowDrawPanel_SelectionChanged;
                    currentSources = flowDrawDockForm.FlowDrawPanel.GetSources();
                    foreach (var source in currentSources)
                    {
                        source.PropertyChanged += Source_PropertyChanged;
                    }
                }
                toolStripButton1.IsEnabled = toolStripButton2.IsEnabled = true;
            });
        }

        void Source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBreakPointSet")
            {
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
        }

        void FlowDrawPanel_SelectionChanged(object sender, EventArgs e)
        {
            var flowDrawPanel = (FlowDrawPanel)sender;
            var ids = flowDrawPanel.GetSelectedSourceId();
            if (ids.Length == 1)
            {
                controllerHost.ChangeSource(currentFileName, ids[0]);
            }
        }

        void ControllerHost_SourceChanged(string arg1, string arg2)
        {
            Dispatcher.Invoke(() =>
            {
                var flowDrawDockForm = GetFlowDrawDockForm(arg2);
                if (flowDrawDockForm == null)
                {
                    var filePath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}", WindowUtility.MainForm.CurrentDifficulty, arg2));
                    filePath = Path.ChangeExtension(filePath, ".fsml");
                    flowDrawDockForm = AddFlowDrawForm(filePath);
                }
                if (flowDrawDockForm != null)
                {
                    flowDrawDockForm.Activate();
                    var lastSourceId = executingPropertyPanel.SourceId;
                    executingPropertyPanel.ChangeSource(arg2, arg1);
                    if (lastSourceId != executingPropertyPanel.SourceId)
                    {
                        flowDrawDockForm.FlowDrawPanel.SelectAndFocus(executingPropertyPanel.SourceId);
                    }
                }
                ShowDock();
                WindowUtility.MainForm.Activate();
            });
        }

        void FlowSearchPanel_SelectionChanged(SearchResultItem obj)
        {
            var foundForm = FlowDrawForms.FirstOrDefault(f => f.FlowDrawPanel.Guid == obj.SearchResult.Guid);
            if (foundForm == null)
            {
                return;
            }
            if (CurrentFlowDrawForm != foundForm)
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

        SearchResult FlowSearchPanel_Searched(string arg)
        {
            if (CurrentFlowDrawForm == null)
            {
                return null;
            }

            return CurrentFlowDrawForm.FlowDrawPanel.Search(arg);
        }

        FlowDrawDocument lastActiveDocument;

        void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            // スクリプトのタブを切り替えたら、前のタブのノードのプロパティは消す
            var current = CurrentFlowDrawForm;
            if (current != lastActiveDocument)
            {
                lastActiveDocument = current;
                propertyPanel.MakeEmpty();
            }
        }

        private FlowDrawDocument AddFlowDrawForm(string filePath)
        {
            FlowDrawDocument drawForm = null;
            if (!File.Exists(filePath))
            {
                return null;
            }
            if ((drawForm = FlowDrawForms.FirstOrDefault(f => f.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase))) != null)
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

        public void SetLang()
        {
            this.編集MenuItem.Header = Utility.Language["Edit"];
            this.元に戻すMenuItem.Header = Utility.Language["Undo"];
            this.やり直すMenuItem.Header = Utility.Language["Redo"];
            this.表示MenuItem.Header = Utility.Language["Show"];
            this.切り取りMenuItem.Header = Utility.Language["Cut"];
            this.コピーMenuItem.Header = Utility.Language["Copy"];
            this.貼り付けMenuItem.Header = Utility.Language["Paste"];
            this.リンクも貼り付けMenuItem.Header = Utility.Language["PasteWithLinks"];
            this.削除MenuItem.Header = Utility.Language["Delete"];
            this.表示領域を合わせるMenuItem.Header = Utility.Language["FitView"];
            this.全て展開MenuItem.Header = Utility.Language["ExpandAll"];
            this.全て縮小MenuItem.Header = Utility.Language["CollapseAll"];
            this.toolStripButton1.ToolTip = Utility.Language["Continue"];
            this.toolStripButton2.ToolTip = Utility.Language["StepIn"];
            this.保存MenuItem.Header = Utility.Language["Save"];
            SetAnchorableTitles();
            scriptListForm.SetLang();
            logForm.SetLang();

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

        void DrawForm_Modified(object sender, EventArgs e)
        {
            var drawForm = (FlowDrawDocument)sender;
            if (!IsUnderFunctions(drawForm.FilePath) && !IsUnderCommands(drawForm.FilePath))
            {
                ContentChanged();
            }
        }

        private static bool IsUnderFunctions(string filePath)
        {
            return IsChildFolderOrFile(Path.Combine(Utility.AppDir, "Functions"), filePath);
        }

        private static bool IsUnderCommands(string filePath)
        {
            return IsChildFolderOrFile(Path.Combine(Utility.AppDir, "Commands"), filePath);
        }

        private static bool IsChildFolderOrFile(string parent, string child)
        {
            return child.Contains(parent, StringComparison.CurrentCultureIgnoreCase);
        }

        void ScriptListForm_ScriptSelected(string obj)
        {
            AddFlowDrawForm(obj);
        }

        void ScriptListForm_Reloaded()
        {
            foreach (FlowDrawDocument drawForm in FlowDrawForms.Where(d => !File.Exists(d.FilePath)))
            {
                drawForm.Close();
            }
        }

        void ScriptListForm_Renamed(string arg1, string arg2)
        {
            if (Path.GetExtension(arg1) != "")
            {
                // File Change
                foreach (FlowDrawDocument drawForm in FlowDrawForms)
                {
                    if (drawForm.FilePath.Equals(arg1, StringComparison.CurrentCultureIgnoreCase))
                    {
                        drawForm.FilePath = arg2;
                    }
                }
            }
            else
            {
                // Folder Change
                foreach (FlowDrawDocument drawForm in FlowDrawForms)
                {
                    if (IsChildFolderOrFile(arg1, drawForm.FilePath))
                    {
                        drawForm.FilePath = Path.Combine(arg2, drawForm.FilePath[(arg1.Length + 1)..]);
                    }
                }
            }
        }

        void ScriptListForm_Deleted(string obj)
        {
            var removeTargets = new List<FlowDrawDocument>();
            foreach (FlowDrawDocument drawForm in FlowDrawForms)
            {
                if (drawForm.FilePath.Equals(obj, StringComparison.CurrentCultureIgnoreCase) || IsChildFolderOrFile(obj, drawForm.FilePath))
                {
                    removeTargets.Add(drawForm);
                }
            }
            foreach (FlowDrawDocument removeTarget in removeTargets)
            {
                removeTarget.Close();
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
            foreach (FlowDrawDocument drawForm in FlowDrawForms)
            {
                if (!IsUnderFunctions(drawForm.FilePath))
                {
                    drawForm.Close();
                }
            }
        }

        public void SaveAll()
        {
            foreach (FlowDrawDocument drawForm in FlowDrawForms)
            {
                if (drawForm.IsContentChanged && !IsUnderFunctions(drawForm.FilePath) && !IsUnderCommands(drawForm.FilePath))
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
            this.スクリプトリストMenuItem.IsChecked = IsShown(scriptListAnchorable);
            this.ソースリストMenuItem.IsChecked = IsShown(treeViewAnchorable);
            this.プロパティMenuItem.IsChecked = IsShown(propertyAnchorable);
            this.検索MenuItem.IsChecked = IsShown(searchAnchorable);
            this.デバッグMenuItem.IsChecked = IsShown(executingPropertyAnchorable);
            this.ログMenuItem.IsChecked = IsShown(logAnchorable);
        }

        private static bool IsShown(LayoutAnchorable anchorable)
        {
            return anchorable.IsVisible && !anchorable.IsAutoHidden;
        }

        /// <summary>
        /// ウィンドウを出すか隠す。出ていて前にあるときは隠し、ほかのウィンドウの後ろにあるときは前に出す。Shift を押していると浮かせて出す。
        /// </summary>
        private static void ShowOrHideWindow(LayoutAnchorable anchorable)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                anchorable.Show();
                anchorable.Float();
                anchorable.IsActive = true;
                return;
            }
            if (!anchorable.IsVisible || anchorable.IsAutoHidden)
            {
                anchorable.Show();
                anchorable.IsActive = true;
            }
            else if (anchorable.IsActive || anchorable.IsFloating)
            {
                anchorable.Hide();
            }
            else
            {
                anchorable.IsActive = true;
            }
        }

        private void プロパティMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideWindow(propertyAnchorable);
        }

        private void スクリプトリストMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideWindow(scriptListAnchorable);
        }

        private void ソースリストMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideWindow(treeViewAnchorable);
        }

        private void 検索MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideWindow(searchAnchorable);
        }

        private void デバッグMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideWindow(executingPropertyAnchorable);
        }

        private void ログMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideWindow(logAnchorable);
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
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }
            var panel = CurrentFlowDrawForm.FlowDrawPanel;
            var modifiers = Keyboard.Modifiers;
            Action action = (modifiers, e.Key) switch
            {
                (ModifierKeys.Control, Key.Z) => panel.Undo,
                (ModifierKeys.Control, Key.Y) => panel.Redo,
                (ModifierKeys.Control, Key.X) => panel.Cut,
                (ModifierKeys.Control, Key.C) => panel.Copy,
                (ModifierKeys.Control, Key.V) => panel.PasteAtCursor,
                (ModifierKeys.Control | ModifierKeys.Shift, Key.V) => panel.PasteWithLinksAtCursor,
                (ModifierKeys.None, Key.Delete) => panel.Delete,
                _ => null,
            };
            if (action != null)
            {
                action();
                e.Handled = true;
            }
        }

        private void 元に戻すMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Undo();
        }

        private void やり直すMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Redo();
        }

        private void 切り取りMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Cut();
        }

        private void コピーMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Copy();
        }

        private void 貼り付けMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.PasteAtCursor();
        }

        private void リンクも貼り付けMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.PasteWithLinksAtCursor();
        }

        private void 削除MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Delete();
        }

        private void 表示領域を合わせるMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawForm?.FlowDrawPanel.FitToView();
        }

        private void 全て展開MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawForm?.FlowDrawPanel.ExpandAll();
        }

        private void 全て縮小MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawForm?.FlowDrawPanel.CollapseAll();
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

        private static Regex NamespaceFilter(string ns)
        {
            return new Regex("^" + Regex.Escape(ns) + "\\.");
        }

        [GeneratedRegex("^PPD\\.Mod")]
        private static partial Regex PPDModRegex();
    }
}
