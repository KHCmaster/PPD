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
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public partial class ScriptManager : ChangableDockContent
    {
        static FlowPropertyDockForm propertyForm;
        static FlowTreeViewDockForm treeViewForm;
        static ScriptListDockForm scriptListForm;
        static FlowSearchDockForm searchForm;
        static FlowExecutingPropertyDockForm executingPropertyForm;
        static FlowLogDockForm logForm;
        private TcpDebugControllerHost controllerHost;
        private Source[] currentSources;
        private string currentFileName;

        public FlowDrawDockForm[] FlowDrawForms
        {
            get
            {
                return dockPanel1.Documents.OfType<FlowDrawDockForm>().ToArray();
            }
        }

        public FlowDrawDockForm CurrentFlowDrawForm
        {
            get
            {
                return dockPanel1.ActiveDocument as FlowDrawDockForm;
            }
        }

        public ScriptManager()
        {
            InitializeComponent();

            propertyForm = new FlowPropertyDockForm();
            treeViewForm = new FlowTreeViewDockForm();
            scriptListForm = new ScriptListDockForm();
            searchForm = new FlowSearchDockForm();
            executingPropertyForm = new FlowExecutingPropertyDockForm();
            logForm = new FlowLogDockForm();

            propertyForm.VisibleChanged += windowVisibleChanged;
            treeViewForm.VisibleChanged += windowVisibleChanged;
            scriptListForm.VisibleChanged += windowVisibleChanged;
            searchForm.VisibleChanged += windowVisibleChanged;
            executingPropertyForm.VisibleChanged += windowVisibleChanged;
            logForm.VisibleChanged += windowVisibleChanged;

            scriptListForm.Renamed += scriptListForm_Renamed;
            scriptListForm.Deleted += scriptListForm_Deleted;
            scriptListForm.ScriptSelected += scriptListForm_ScriptSelected;
            scriptListForm.Reloaded += scriptListForm_Reloaded;

            searchForm.FlowSearchPanel.Searched += FlowSearchPanel_Searched;
            searchForm.FlowSearchPanel.SelectionChanged += FlowSearchPanel_SelectionChanged;

            dockPanel1.ActiveDocumentChanged += dockPanel1_ActiveDocumentChanged;

            FlowDrawPanel.EnumerateClasses("dlls", new string[] { "FlowScriptEngineConsole" });
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD", new Regex[] {
                new Regex("^FlowScriptEngineBasic\\."),
                new Regex("^FlowScriptEngineBasicExtension\\."),
                new Regex("^FlowScriptEngineSlimDX\\."),
                new Regex("^FlowScriptEnginePPD\\."),
                new Regex("^FlowScriptEngineData\\.")},
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Script)", new Regex[] {
                new Regex("^FlowScriptEngineBasic\\."),
                new Regex("^FlowScriptEngineBasicExtension\\."),
                new Regex("^FlowScriptEngineSlimDX\\."),
                new Regex("^FlowScriptEnginePPD\\."),
                new Regex("^FlowScriptEngineData\\.")},
                null, null, new Regex[] { new Regex("^PPD\\.Mod") }));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Mod)", new Regex[] {
                new Regex("^FlowScriptEngineBasic\\."),
                new Regex("^FlowScriptEngineBasicExtension\\."),
                new Regex("^FlowScriptEngineSlimDX\\."),
                new Regex("^FlowScriptEnginePPD\\."),
                new Regex("^FlowScriptEngineData\\.")},
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPDEditor", new Regex[] {
                new Regex("^FlowScriptEngineBasic\\."),
                new Regex("^FlowScriptEngineBasicExtension\\."),
                new Regex("^FlowScriptEngineSlimDX\\."),
                new Regex("^FlowScriptEnginePPDEditor\\."),
                new Regex("^FlowScriptEngineData\\.")},
                null));

            Load += ScriptManager_Load;
        }

        void ScriptManager_Load(object sender, EventArgs e)
        {
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
                    if (!IsUnderFunctions(flowDrawForm.FilePath) && filePath.IndexOf(scriptFilePath.ToLower()) >= 0)
                    {
                        breakPoints = flowDrawForm.FlowDrawPanel.GetBreakPoints();
                        break;
                    }
                }
                if (breakPoints == null)
                {
                    breakPoints = new int[0];
                }
                infos.Add(scriptFilePath, breakPoints);
            }
            EnableControllerHost(infos);
        }

        private void EnableControllerHost(Dictionary<string, int[]> infos)
        {
            toolStrip1.Visible = true;
            toolStripButton1.Enabled = toolStripButton2.Enabled = false;
            controllerHost = new TcpDebugControllerHost(infos);
            controllerHost.SourceChanged += controllerHost_SourceChanged;
            controllerHost.OperationWaited += controllerHost_OperationWaited;
            controllerHost.OperationAccepted += controllerHost_OperationAccepted;
            controllerHost.ErrorOccurred += controllerHost_ErrorOccurred;
            controllerHost.LogReceived += ControllerHost_LogReceived;
            controllerHost.Create();
        }

        private void DisableControllerHost()
        {
            toolStrip1.Visible = false;
            if (controllerHost != null)
            {
                controllerHost.Close();
            }
            foreach (var flowDrawDockForm in FlowDrawForms)
            {
                flowDrawDockForm.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
                currentSources = flowDrawDockForm.FlowDrawPanel.GetSources();
                foreach (var source in currentSources)
                {
                    source.PropertyChanged -= source_PropertyChanged;
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

        private FlowDrawDockForm GetFlowDrawDockForm(string fileName)
        {
            return FlowDrawForms.FirstOrDefault(f => f.FilePath.ToLower().IndexOf(fileName.ToLower()) >= 0);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            controllerHost.Continue();
            toolStripButton1.Enabled = toolStripButton2.Enabled = false;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            controllerHost.StepIn();
            toolStripButton1.Enabled = toolStripButton2.Enabled = false;
        }

        private void ControllerHost_LogReceived(string arg1, string arg2)
        {
            this.Invoke((Action)(() =>
            {
                logForm.AddLog(arg1, arg2);
            }));
        }

        void controllerHost_ErrorOccurred(string arg1, int arg2, string arg3)
        {
            this.Invoke((Action)(() =>
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
                    MessageBox.Show(arg3);
                }
            }));
        }

        void controllerHost_OperationAccepted(string obj)
        {
            this.Invoke((Action)(() =>
            {
                var flowDrawDockForm = GetFlowDrawDockForm(obj);
                if (flowDrawDockForm != null)
                {
                    flowDrawDockForm.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
                }
                if (currentSources != null)
                {
                    foreach (var source in currentSources)
                    {
                        source.PropertyChanged -= source_PropertyChanged;
                    }
                    currentSources = null;
                }
                toolStripButton1.Enabled = toolStripButton2.Enabled = false;
            }));
        }

        void controllerHost_OperationWaited(string obj)
        {
            this.Invoke((Action)(() =>
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
                        source.PropertyChanged += source_PropertyChanged;
                    }
                }
                toolStripButton1.Enabled = toolStripButton2.Enabled = true;
            }));
        }

        void source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        void controllerHost_SourceChanged(string arg1, string arg2)
        {
            this.Invoke((Action)(() =>
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
                    var lastSourceId = executingPropertyForm.FlowExecutingPropertyPanel.SourceId;
                    executingPropertyForm.FlowExecutingPropertyPanel.ChangeSource(arg2, arg1);
                    if (lastSourceId != executingPropertyForm.FlowExecutingPropertyPanel.SourceId)
                    {
                        flowDrawDockForm.FlowDrawPanel.SelectAndFocus(executingPropertyForm.FlowExecutingPropertyPanel.SourceId);
                    }
                }
                this.Activate();
                WindowUtility.MainForm.Activate();
            }));
        }

        void FlowSearchPanel_SelectionChanged(FlowScriptControl.Classes.SearchResultItem obj)
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

        void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            propertyForm.FlowPropertyPanel.MakeEmpty();
        }

        private FlowDrawDockForm AddFlowDrawForm(string filePath)
        {
            FlowDrawDockForm drawForm = null;
            if (!File.Exists(filePath))
            {
                return null;
            }
            if ((drawForm = FlowDrawForms.FirstOrDefault(f => f.FilePath.ToLower() == filePath.ToLower())) != null)
            {
                drawForm.Activate();
                return drawForm;
            }

            drawForm = new FlowDrawDockForm();
            drawForm.VisibleChanged += windowVisibleChanged;
            drawForm.Modified += drawForm_Modified;
            drawForm.FlowDrawPanel.SearchRequired += FlowDrawPanel_SearchRequired;
            drawForm.FlowDrawPanel.FlowPropertyPanel = propertyForm.FlowPropertyPanel;
            drawForm.FlowDrawPanel.FlowTreeView = treeViewForm.FlowTreeView;
            drawForm.SetLang();
            drawForm.FilePath = filePath;
            drawForm.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Document);
            return drawForm;
        }

        void FlowDrawPanel_SearchRequired(string obj)
        {
            searchForm.FlowSearchPanel.SearchQuery = obj;
            searchForm.Activate();
        }

        public void SetLang()
        {
            this.編集ToolStripMenuItem.Text = Utility.Language["Edit"];
            this.元に戻すToolStripMenuItem.Text = Utility.Language["Undo"];
            this.やり直すToolStripMenuItem.Text = Utility.Language["Redo"];
            this.表示ToolStripMenuItem.Text = Utility.Language["Show"];
            this.切り取りToolStripMenuItem.Text = Utility.Language["Cut"];
            this.コピーToolStripMenuItem.Text = Utility.Language["Copy"];
            this.貼り付けToolStripMenuItem.Text = Utility.Language["Paste"];
            this.リンクも貼り付けToolStripMenuItem.Text = Utility.Language["PasteWithLinks"];
            this.削除ToolStripMenuItem.Text = Utility.Language["Delete"];
            this.表示領域を合わせるToolStripMenuItem.Text = Utility.Language["FitView"];
            this.全て展開ToolStripMenuItem.Text = Utility.Language["ExpandAll"];
            this.全て縮小ToolStripMenuItem.Text = Utility.Language["CollapseAll"];
            this.toolStripButton1.Text = Utility.Language["Continue"];
            this.toolStripButton2.Text = Utility.Language["StepIn"];
            propertyForm.TabText = this.プロパティToolStripMenuItem.Text = Utility.Language["NodePropertyWindow"];
            treeViewForm.TabText = this.ソースリストToolStripMenuItem.Text = Utility.Language["NodeListWindow"];
            scriptListForm.TabText = this.スクリプトリストToolStripMenuItem.Text = Utility.Language["ScriptListWindow"];
            searchForm.TabText = this.検索ToolStripMenuItem.Text = Utility.Language["NodeSearchWindow"];
            executingPropertyForm.TabText = this.デバッグToolStripMenuItem.Text = Utility.Language["Debug"];
            logForm.TabText = this.ログToolStripMenuItem.Text = Utility.Language["Log"];
            scriptListForm.SetLang();
            logForm.SetLang();

            foreach (FlowDrawDockForm dockForm in FlowDrawForms)
            {
                dockForm.SetLang();
            }
        }

        void drawForm_Modified(object sender, EventArgs e)
        {
            var drawForm = sender as FlowDrawDockForm;
            if (!IsUnderFunctions(drawForm.FilePath) && !IsUnderCommands(drawForm.FilePath))
            {
                ContentChanged();
            }
        }

        private bool IsUnderFunctions(string filePath)
        {
            return IsChildFolderOrFile(Path.Combine(Utility.AppDir, "Functions"), filePath);
        }

        private bool IsUnderCommands(string filePath)
        {
            return IsChildFolderOrFile(Path.Combine(Utility.AppDir, "Commands"), filePath);
        }

        private bool IsChildFolderOrFile(string parent, string child)
        {
            return child.ToLower().Contains(parent.ToLower());
        }

        void scriptListForm_ScriptSelected(string obj)
        {
            AddFlowDrawForm(obj);
        }

        void scriptListForm_Reloaded()
        {
            foreach (FlowDrawDockForm drawForm in FlowDrawForms.Where(d => !File.Exists(d.FilePath)))
            {
                drawForm.Close();
            }
        }

        void scriptListForm_Renamed(string arg1, string arg2)
        {
            if (Path.GetExtension(arg1) != "")
            {
                // File Change
                foreach (FlowDrawDockForm drawForm in FlowDrawForms)
                {
                    if (drawForm.FilePath.ToLower() == arg1.ToLower())
                    {
                        drawForm.FilePath = arg2;
                    }
                }
            }
            else
            {
                // Folder Change
                foreach (FlowDrawDockForm drawForm in FlowDrawForms)
                {
                    if (IsChildFolderOrFile(arg1, drawForm.FilePath))
                    {
                        drawForm.FilePath = Path.Combine(arg2, drawForm.FilePath.Substring(arg1.Length + 1));
                    }
                }
            }
        }

        void scriptListForm_Deleted(string obj)
        {
            var removeTargets = new List<FlowDrawDockForm>();
            foreach (FlowDrawDockForm drawForm in FlowDrawForms)
            {
                if (drawForm.FilePath.ToLower() == obj.ToLower() || IsChildFolderOrFile(obj, drawForm.FilePath))
                {
                    removeTargets.Add(drawForm);
                }
            }
            foreach (FlowDrawDockForm removeTarget in removeTargets)
            {
                removeTarget.Close();
            }
        }

        public void RestoreDock(string filePath)
        {
            if (File.Exists(filePath))
            {
                dockPanel1.LoadFromXml(filePath, DeserializeForm);
            }
            else
            {
                propertyForm.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight);
                searchForm.Show(propertyForm.Pane, WeifenLuo.WinFormsUI.Docking.DockAlignment.Top, 0.5);
                treeViewForm.Show(searchForm.Pane, searchForm);
                scriptListForm.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockLeft);
                logForm.Show(scriptListForm.Pane, DockAlignment.Bottom, 0.5);
            }
        }

        public void SaveDock(string filePath)
        {
            dockPanel1.SaveAsXml(filePath);
        }

        public void Clear()
        {
            scriptListForm.Clear();
            foreach (FlowDrawDockForm drawForm in FlowDrawForms)
            {
                if (!IsUnderFunctions(drawForm.FilePath))
                {
                    drawForm.Close();
                }
            }
        }

        public void SaveAll()
        {
            foreach (FlowDrawDockForm drawForm in FlowDrawForms)
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

        private IDockContent DeserializeForm(string persistentString)
        {
            switch (persistentString)
            {
                case "PPDEditor.DockForm.Script.FlowPropertyDockForm":
                    return propertyForm;
                case "PPDEditor.DockForm.Script.FlowTreeViewDockForm":
                    return treeViewForm;
                case "PPDEditor.DockForm.Script.ScriptListDockForm":
                    return scriptListForm;
                case "PPDEditor.DockForm.Script.FlowSearchDockForm":
                    return searchForm;
                case "PPDEditor.DockForm.Script.FlowExecutingPropertyDockForm":
                    return executingPropertyForm;
                case "PPDEditor.DockForm.Script.FlowLogDockForm":
                    return logForm;
            }
            return null;
        }

        private void windowVisibleChanged(object sender, EventArgs e)
        {
            this.スクリプトリストToolStripMenuItem.Checked = Utility.CheckVisible(scriptListForm);
            this.ソースリストToolStripMenuItem.Checked = Utility.CheckVisible(treeViewForm);
            this.プロパティToolStripMenuItem.Checked = Utility.CheckVisible(propertyForm);
            this.検索ToolStripMenuItem.Checked = Utility.CheckVisible(searchForm);
            this.デバッグToolStripMenuItem.Checked = Utility.CheckVisible(executingPropertyForm);
            this.ログToolStripMenuItem.Checked = Utility.CheckVisible(logForm);
        }

        private void プロパティToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, propertyForm, ModifierKeys);
        }

        private void スクリプトリストToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, scriptListForm, ModifierKeys);
        }

        private void ソースリストToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, treeViewForm, ModifierKeys);
        }

        private void 検索ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, searchForm, ModifierKeys);
        }

        private void デバッグToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, executingPropertyForm, ModifierKeys);
        }

        private void ログToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, logForm, ModifierKeys);
        }

        private void 元に戻すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Undo();
        }

        private void やり直すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Redo();
        }

        private void 切り取りToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Cut();
        }

        private void コピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Copy();
        }

        private void 貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            var p = CurrentFlowDrawForm.FlowDrawPanel.PointToClient(System.Windows.Forms.Cursor.Position);
            CurrentFlowDrawForm.FlowDrawPanel.PasteAt(p.X, p.Y);
        }

        private void リンクも貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            var p = CurrentFlowDrawForm.FlowDrawPanel.PointToClient(System.Windows.Forms.Cursor.Position);
            CurrentFlowDrawForm.FlowDrawPanel.PasteWithLinksAt(p.X, p.Y);
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null || !CurrentFlowDrawForm.FlowDrawPanel.IsControlFocused)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.Delete();
        }

        private void 表示領域を合わせるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.FitToView();
        }

        private void 全て展開ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.ExpandAll();
        }

        private void 全て縮小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawForm == null)
            {
                return;
            }

            CurrentFlowDrawForm.FlowDrawPanel.CollapseAll();
        }
    }
}
