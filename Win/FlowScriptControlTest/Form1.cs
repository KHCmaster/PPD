using FlowScriptControl.Classes;
using FlowScriptControl.Controls;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FlowScriptControlTest
{
    public partial class Form1 : Form
    {
        public FlowDrawTab CurrentFlowDrawTab
        {
            get
            {
                return tabControl1.SelectedTab as FlowDrawTab;
            }
        }

        public FlowDrawPanel CurrentFlowDrawPanel
        {
            get
            {
                return CurrentFlowDrawTab.FlowDrawPanel;
            }
        }

        public FlowDrawTab[] Tabs
        {
            get
            {
                var ret = new List<FlowDrawTab>();
                foreach (FlowDrawTab tab in tabControl1.TabPages)
                {
                    ret.Add(tab);
                }
                return ret.ToArray();
            }
        }

        FlowScriptLanguageProvider langProvider;
        Executor currentExecutor;
        ConsoleWriter consoleWriter;
        string currentStdinText = "";

        public Form1()
        {
            InitializeComponent();
            LoadLang();
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
        }

        private void LoadLang()
        {
            var filePath = Path.Combine("Lang", "lang_FlowScript_jp.ini");
            if (!File.Exists(filePath))
            {
                return;
            }

            var flowLang = new SettingReader(File.ReadAllText(filePath));
            filePath = Path.Combine("Lang", "lang_PPDEditor_jp.ini");
            if (!File.Exists(filePath))
            {
                return;
            }
            var lang = new SettingReader(File.ReadAllText(filePath));
            langProvider = new FlowScriptLanguageProvider(lang, flowLang);
        }

        void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            flowPropertyPanel1.MakeEmpty();
        }

        private void Cut()
        {
            CurrentFlowDrawPanel.Cut();
        }

        private void Copy()
        {
            CurrentFlowDrawPanel.Copy();
        }

        private void Undo()
        {
            CurrentFlowDrawPanel.Undo();
        }

        private void Redo()
        {
            CurrentFlowDrawPanel.Redo();
        }

        private void Paste()
        {
            var p = CurrentFlowDrawPanel.PointToClient(Cursor.Position);
            CurrentFlowDrawPanel.PasteAt(p.X, p.Y);
        }

        private void PasteWithLinks()
        {
            var p = CurrentFlowDrawPanel.PointToClient(Cursor.Position);
            CurrentFlowDrawPanel.PasteWithLinksAt(p.X, p.Y);
        }

        private void Delete()
        {
            CurrentFlowDrawPanel.Delete();
        }

        private void AdjustPosition()
        {
            CurrentFlowDrawPanel.AdjustPosition();
        }

        private void 新規作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddEmptyFlowDrawPanel();
        }

        private void 元に戻すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null || !CurrentFlowDrawPanel.IsControlFocused)
            {
                return;
            }
            Undo();
        }

        private void やり直すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null || !CurrentFlowDrawPanel.IsControlFocused)
            {
                return;
            }
            Redo();
        }

        private void コピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null || !CurrentFlowDrawPanel.IsControlFocused)
            {
                return;
            }
            Copy();
        }

        private void 貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null || !CurrentFlowDrawPanel.IsControlFocused)
            {
                return;
            }
            Paste();
        }

        private void リンクも貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null || !CurrentFlowDrawPanel.IsControlFocused)
            {
                return;
            }
            PasteWithLinks();
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null || !CurrentFlowDrawPanel.IsControlFocused)
            {
                return;
            }
            Delete();
        }

        private void 位置を自動調整ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null)
            {
                return;
            }
            AdjustPosition();
        }

        private void 開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null)
            {
                return;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    CurrentFlowDrawPanel.LoadXML(sr.BaseStream);
                    CurrentFlowDrawTab.FilePath = openFileDialog1.FileName;
                }
            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawTab == null)
            {
                return;
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName))
                {
                    CurrentFlowDrawPanel.SaveXML(writer.BaseStream);
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
            {
                return;
            }

            var executeInfos = new List<ExecuteInfo>();
            foreach (FlowDrawTab flowDrawTab in tabControl1.TabPages)
            {
                var stream = new MemoryStream();
                flowDrawTab.FlowDrawPanel.SaveXML(stream);
                var breakPoints = flowDrawTab.FlowDrawPanel.GetBreakPoints();
                stream.Seek(0, SeekOrigin.Begin);
                executeInfos.Add(new ExecuteInfo(stream, breakPoints, flowDrawTab, flowDrawTab.FlowDrawPanel.GetSources()));
            }
            consoleWriter = new ConsoleWriter();
            consoleWriter.Output += consoleWriter_Output;
            currentExecutor = new Executor(executeInfos.ToArray(), currentStdinText);
            currentExecutor.Finished += executor_Finished;
            currentExecutor.Aborted += executor_Aborted;
            toolStripButton1.Enabled = false;
            toolStrip2.Visible = true;
            toolStripButton2.Enabled = toolStripButton4.Enabled = false;
            currentExecutor.Controller.SourceChanged += Controller_SourceChanged;
            currentExecutor.Controller.OperationAccepted += Controller_OperationAccepted;
            currentExecutor.Controller.OperationWaited += Controller_OperationWaited;
            currentExecutor.Controller.ErrorOccurred += Controller_ErrorOccurred;
            currentExecutor.Execute();
        }

        void Controller_ErrorOccurred(FlowSourceManager arg1, FlowExecutionException arg2)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(arg1);
            this.Invoke((Action)(() =>
            {
                if (executeInfo != null)
                {
                    var errorText = arg2.ToString();
                    if (arg2.SourceObject != null)
                    {
                        executeInfo.FlowDrawTab.FlowDrawPanel.ShowError(arg2.SourceObject.Id, errorText);
                    }
                    else
                    {
                        MessageBox.Show(errorText);
                    }
                }
            }));
        }

        void consoleWriter_Output(string obj)
        {
            this.Invoke((Action)(() =>
            {
                textBox1.AppendText(obj);
            }));
        }

        void Controller_OperationWaited(FlowSourceManager obj)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(obj);
            if (executeInfo != null)
            {
                executeInfo.FlowDrawTab.FlowDrawPanel.SelectionChanged += FlowDrawPanel_SelectionChanged;
            }
            this.Invoke((Action)(() =>
            {
                toolStripButton2.Enabled = toolStripButton4.Enabled = true;
            }));
        }

        void Controller_OperationAccepted(FlowSourceManager obj)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(obj);
            if (executeInfo != null)
            {
                executeInfo.FlowDrawTab.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
            }
            this.Invoke((Action)(() =>
            {
                toolStripButton2.Enabled = toolStripButton4.Enabled = false;
            }));
        }

        void Controller_SourceChanged(string arg1, FlowSourceManager arg2)
        {
            this.Invoke((Action)(() =>
            {
                var executeInfo = currentExecutor.GetExecuteInfo(arg2);
                if (executeInfo != null)
                {
                    var tab = Tabs.FirstOrDefault(t => t == executeInfo.FlowDrawTab);
                    tabControl1.SelectedTab = tab;
                    var lastSourceId = flowExecutingPropertyPanel1.SourceId;
                    flowExecutingPropertyPanel1.ChangeSource(executeInfo.FlowDrawTab.FilePath, arg1);
                    if (lastSourceId != flowExecutingPropertyPanel1.SourceId)
                    {
                        tab.FlowDrawPanel.SelectAndFocus(flowExecutingPropertyPanel1.SourceId);
                    }
                }
            }));
        }

        void FlowDrawPanel_SelectionChanged(object sender, EventArgs e)
        {
            var flowDrawPanel = (FlowDrawPanel)sender;
            var ids = flowDrawPanel.GetSelectedSourceId();
            if (ids.Length == 1)
            {
                currentExecutor.Controller.ChangeSource(ids[0]);
            }
        }

        void executor_Finished()
        {
            this.Invoke((Action)(() =>
            {
                toolStripButton1.Enabled = true;
                toolStrip2.Visible = false;
                if (consoleWriter != null)
                {
                    consoleWriter.Close();
                    consoleWriter = null;
                }
            }));
        }

        void executor_Aborted()
        {
            this.Invoke((Action)(() =>
            {
                MessageBox.Show("スクリプトのプロセスが終了しなかったためKillしました。");
                if (consoleWriter != null)
                {
                    consoleWriter.Stop();
                    consoleWriter = null;
                }
            }));
        }

        private void AddEmptyFlowDrawPanel()
        {
            var flowDrawTab = new FlowDrawTab();
            flowDrawTab.FlowDrawPanel.SearchRequired += FlowDrawPanel_SearchRequired;
            flowDrawTab.FlowDrawPanel.FlowTreeView = flowTreeView1;
            flowDrawTab.FlowDrawPanel.FlowPropertyPanel = flowPropertyPanel1;
            if (langProvider != null)
            {
                flowDrawTab.FlowDrawPanel.LanguageProvider = langProvider;
            }
            tabControl1.TabPages.Add(flowDrawTab);
            tabControl1.SelectedTab = flowDrawTab;
        }

        void FlowDrawPanel_SearchRequired(string obj)
        {
            flowSearchPanel1.SearchQuery = obj;
            tabControl2.SelectedTab = tabPage2;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FlowDrawPanel.EnumerateClasses("dlls", new string[0]);
            FlowDrawPanel.AddFilter(new NodeFilterInfo("Console", new Regex[] {
                new Regex("^FlowScriptEngineBasic\\."),
                new Regex("^FlowScriptEngineBasicExtension\\."),
                new Regex("^FlowScriptEngineConsole\\."),
                new Regex("^FlowScriptEngineData\\.")},
                null));
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
            AddEmptyFlowDrawPanel();
            flowSearchPanel1.Searched += flowSearchPanel1_Searched;
            flowSearchPanel1.SelectionChanged += flowSearchPanel1_SelectionChanged;
        }

        void flowSearchPanel1_SelectionChanged(FlowScriptControl.Classes.SearchResultItem obj)
        {
            var foundTab = Tabs.FirstOrDefault(tab => tab.FlowDrawPanel.Guid == obj.SearchResult.Guid);
            if (foundTab == null)
            {
                return;
            }
            if (CurrentFlowDrawTab != foundTab)
            {
                tabControl1.SelectedTab = foundTab;
            }

            if (obj.Source == null)
            {
                foundTab.FlowDrawPanel.SelectAndFocus(obj.Comment);
            }
            else
            {
                foundTab.FlowDrawPanel.SelectAndFocus(obj.Source);
            }
        }

        SearchResult flowSearchPanel1_Searched(string arg)
        {
            if (CurrentFlowDrawTab == null)
            {
                return null;
            }

            return CurrentFlowDrawPanel.Search(arg);
        }

        private void クリアToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.Clear();
            textBox1.Clear();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            currentExecutor.Controller.Continue();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            currentExecutor.Abort();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            currentExecutor.Controller.StepIn();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (currentExecutor != null && currentExecutor.IsExecuting)
            {
                if (MessageBox.Show("実行中のスクリプトがあります。終了しますか？", "確認", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                currentExecutor.Finished -= executor_Finished;
                currentExecutor.Aborted -= executor_Aborted;
                currentExecutor.Abort();
            }
        }

        private void 全て展開ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawPanel != null)
            {
                CurrentFlowDrawPanel.ExpandAll();
            }
        }

        private void 全て縮小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFlowDrawPanel != null)
            {
                CurrentFlowDrawPanel.CollapseAll();
            }
        }

        private void 標準入力ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new StdinForm
            {
                InputText = currentStdinText
            };
            if (form.ShowDialog() == DialogResult.OK)
            {
                currentStdinText = form.InputText;
            }
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabControl1.TabCount; ++i)
                {
                    if (tabControl1.GetTabRect(i).Contains(tabControl1.PointToClient(Cursor.Position)))
                    {
                        var tab = tabControl1.TabPages[i] as FlowDrawTab;
                        タブを閉じるToolStripMenuItem.Tag = tab;
                        contextMenuStrip1.Show(Cursor.Position);
                        break;
                    }
                }
            }
        }

        private void タブを閉じるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (タブを閉じるToolStripMenuItem.Tag != null)
            {
                var tab = タブを閉じるToolStripMenuItem.Tag as TabPage;
                tabControl1.TabPages.Remove(tab);
                タブを閉じるToolStripMenuItem.Tag = null;
            }
        }
    }
}
