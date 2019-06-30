using PPDUpdater.Controls;
using PPDUpdater.Executor;
using PPDUpdater.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PPDUpdater
{
    [Flags]
    public enum WorkState
    {
        None = 0,
        DownLoad = 1,
        Install = 2,
        DownloadAndInstall = 3
    }
    public partial class Form1 : Form
    {
        const string installinfofile = "install.info";
        const string iniFileName = "PPDUpdater.ini";
        const string logfile = "update.log";

        string askfinish = "未インストールのバージョンがありますが、終了してもよろしいですか？";
        string askfinishcaption = "終了確認";
        string download = "ダウンロード";
        string install = "インストール";
        string alreadylatest = "既に最新版です";
        string updatelistfail = "アップデートリストの取得に失敗しました";

        string waiting = "待機中";
        string downloading = "ダウンロード中";
        string downloadfinished = "ダウンロード完了";
        string installing = "インストール中";
        string installfinished = "インストール完了";

        string updatefinished = "アップデートが正常に終了しました";

        InstallInfo installInfo = new InstallInfo();
        List<string> updatelist = new List<string>();
        string channel;

        private string langFileName = "";
        private string langFileISO = "";

        public WorkState WorkState
        {
            get;
            set;
        }

        public Form1()
        {
            InitializeComponent();

            WorkState = WorkState.DownloadAndInstall;

            ReadInstallInfo();
            ReadSetting();

            if (!Directory.Exists(Utility.DownloadDirectory))
            {
                Directory.CreateDirectory(Utility.DownloadDirectory);
            }

            var pbColumn = new DataGridViewProgressBarColumn
            {
                Name = download,
                ReadOnly = true
            };
            dataGridView1.Columns.Add(pbColumn);
            pbColumn = new DataGridViewProgressBarColumn
            {
                Name = install,
                ReadOnly = true
            };
            dataGridView1.Columns.Add(pbColumn);

            CheckSetting();
            CheckLangFiles();
            SetLanguage(langFileName);
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var sr = new StreamReader(iniFileName);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                langFileISO = setting["Language"];
                langFileName = Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO));
            }
        }

        private void CheckLangFiles()
        {
            if (Directory.Exists("Lang"))
            {
                foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", this.GetType().Assembly.GetName().Name)))
                {
                    var sr = new StreamReader(fileName);
                    var setting = new SettingReader(sr.ReadToEnd());
                    sr.ReadToEnd();
                    string name = setting["DisplayName"];
                    var tsmi = new ToolStripMenuItem
                    {
                        Text = name,
                        Name = fileName.ToLower(),
                        Checked = Path.GetFileName(fileName).ToLower() == String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO).ToLower()
                    };
                    tsmi.Click += tsmi_Click;
                    言語ToolStripMenuItem.DropDownItems.Add(tsmi);
                }
            }
        }

        void tsmi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                langFileName = tsmi.Name;
                var m = Regex.Match(Path.GetFileName(langFileName), "^lang_\\w+_(?<ISO>\\w+).ini$");
                if (m.Success)
                {
                    langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(langFileName);
                foreach (ToolStripMenuItem child in (tsmi.OwnerItem as ToolStripMenuItem).DropDownItems)
                {
                    child.Checked = false;
                }
                tsmi.Checked = true;
            }
        }


        private void SetLanguage(string fileName)
        {
            if (File.Exists(fileName))
            {
                var sr = new StreamReader(fileName);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                ファイルToolStripMenuItem.Text = setting["File"];
                終了ToolStripMenuItem.Text = setting["Finish"];
                言語ToolStripMenuItem.Text = setting["Language"];
                askfinish = setting["AskFinish"];
                askfinishcaption = setting["AskFinishCaption"];
                download = setting["Download"];
                install = setting["Install"];
                alreadylatest = setting["AlreadyLatest"];
                updatelistfail = setting["UpdateListFail"];
                waiting = setting["Waiting"];
                downloading = setting["Downloading"];
                downloadfinished = setting["DownloadFinished"];
                installing = setting["Installing"];
                installfinished = setting["InstallFinished"];
                updatefinished = setting["UpdateFinished"];
                dataGridView1.Columns[0].HeaderText = setting["Header1"];
                dataGridView1.Columns[1].HeaderText = setting["Header2"];
                dataGridView1.Columns[2].HeaderText = dataGridView1.Columns[2].Name = download;
                dataGridView1.Columns[3].HeaderText = dataGridView1.Columns[3].Name = install;
                label1.Text = setting["Label1"];
                label3.Text = setting["Label3"];
                groupBox1.Text = setting["GroupBox1"];
                button1.Text = setting["Button1"];
                button2.Text = setting["Button2"];
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Load -= Form1_Load;
            if (WorkState == WorkState.DownloadAndInstall)
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.Hide();
                this.Visible = false;
            }

            if (WorkState == WorkState.DownLoad || WorkState == WorkState.DownloadAndInstall)
            {
                ProcessDownloadAndInstall();
            }
            else if (WorkState == WorkState.Install)
            {
                FindDownloadData();
            }

        }

        private void FindDownloadData()
        {
            if (Directory.Exists(Utility.DownloadDirectory))
            {
                foreach (string dir in Directory.GetDirectories(Utility.DownloadDirectory))
                {
                    if (File.Exists(Path.Combine(dir, Utility.Complete)))
                    {
                        foreach (string fn in Directory.GetFiles(dir))
                        {
                            var filename = Path.GetFileName(fn);
                            var m = Utility.UpdateRegex.Match(filename);
                            if (m.Success)
                            {
                                var ui = new UpdateInfo(m.Groups[Utility.UpdateRegexGroup].Value, "")
                                {
                                    FilePath = fn
                                };
                                dataGridView1.Rows.Add(ui, downloadfinished);
                            }
                        }
                    }
                }
                ProcessInstall();
            }
            else
            {
                WriteLog("install:no update");
                this.Close();
            }
        }

        private void ProcessDownload()
        {
            bool downloaded = false;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if ((string)dataGridView1[State.Name, i].Value == waiting)
                {
                    downloaded = true;
                    dataGridView1[State.Name, i].Value = downloading;
                    var ui = dataGridView1[Version.Name, i].Value as UpdateInfo;
                    var due = new DownloadUpdateExecutor(ui, this, i);
                    due.Progressed += due_Progressed;
                    due.Finished += due_Finished;
                    var thread = new Thread(new ThreadStart(due.Execute));
                    WriteLog(String.Format("DownloadUpdate:{0}", due.URL));
                    thread.Start();
                    break;
                }
            }
            if (!downloaded)
            {
                if (WorkState == WorkState.DownloadAndInstall)
                {
                    ProcessInstall();
                }
                else
                {
                    this.Close();
                }
            }
        }

        void due_Finished(object sender, EventArgs e)
        {
            var due = sender as DownloadUpdateExecutor;
            dataGridView1[download, due.Index].Value = 100;
            if (due.Success)
            {
                dataGridView1[State.Name, due.Index].Value = downloadfinished;
                ProcessDownload();
            }
            else
            {
                WriteErrorLog(String.Format("DownloadUpdate:{0}", due.ErrorLog));
            }
        }

        void due_Progressed(object sender, EventArgs e)
        {
            var due = sender as DownloadUpdateExecutor;
            dataGridView1[download, due.Index].Value = due.Progress;
        }

        private void ProcessInstall()
        {
            bool installed = false;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if ((string)dataGridView1[State.Name, i].Value == downloadfinished)
                {
                    installed = true;
                    dataGridView1[State.Name, i].Value = installing;
                    var ui = dataGridView1[Version.Name, i].Value as UpdateInfo;
                    var iue = new InstallUpdateExecutor(ui.FilePath, ui.VersionInfo, installInfo, i, this);
                    iue.Progressed += iue_Progressed;
                    iue.Finished += iue_Finished;
                    var thread = new Thread(new ThreadStart(iue.Execute));
                    WriteLog(String.Format("InstallUpdate:{0}", iue.FilePath));
                    thread.Start();
                    break;
                }
            }
            if (!installed)
            {
                ShowMessageBox(updatefinished);
                if (WorkState == WorkState.Install)
                {
                    this.Close();
                }
            }
        }

        void iue_Progressed(object sender, EventArgs e)
        {
            var iue = sender as InstallUpdateExecutor;
            dataGridView1[install, iue.Index].Value = iue.Progress;
        }

        void iue_Finished(object sender, EventArgs e)
        {
            var iue = sender as InstallUpdateExecutor;
            installInfo.InstallVersion = iue.InstallVersion;
            dataGridView1[install, iue.Index].Value = 100;
            if (iue.Success)
            {
                WriteInstallInfo();
                dataGridView1[State.Name, iue.Index].Value = installfinished;
                ProcessInstall();
            }
            else
            {
                WriteErrorLog(String.Format("InstallUpdate:{0}", iue.ErrorLog));
            }
        }

        private void WriteInstallInfo()
        {
            using (StreamWriter sw = new StreamWriter(installinfofile))
            {
                WriteData(sw, "PPD", installInfo.PPD);
                WriteData(sw, "SharpDX", installInfo.SharpDX);
                WriteData(sw, "DirectShowLib", installInfo.DirectShowLib);
                WriteData(sw, "IPAFont", installInfo.IPAFont);
                WriteData(sw, "PPDeditor", installInfo.PPDeditor);
                WriteData(sw, "ffdshow", installInfo.ffdshow);
                WriteData(sw, "MP4Splitter", installInfo.MP4Splitter);
                WriteData(sw, "FLVSplitter", installInfo.FLVSplitter);
                WriteData(sw, "Effect2DEditor", installInfo.Effect2DEditor);
                WriteData(sw, "PPDVersion", installInfo.PPDVersion.ToString());
                WriteData(sw, "PPDeditorVersion", installInfo.PPDeditorVersion.ToString());
                WriteData(sw, "BMSTOPPDVersion", installInfo.BMSTOPPDVersion.ToString());
                WriteData(sw, "Effect2DEditorVersion", installInfo.Effect2DEditorVersion.ToString());
                WriteData(sw, "InstallVersion", installInfo.InstallVersion.ToString());
            }

        }

        private void WriteData(StreamWriter sw, string key, bool value)
        {
            WriteData(sw, key, value ? "1" : "0");
        }

        private void WriteData(StreamWriter sw, string key, string value)
        {
            sw.Write(String.Format("[{0}]{1}", key, value));
            sw.WriteLine();
        }

        private void ProcessDownloadAndInstall()
        {
            pictureBox1.Visible = true;
            label3.Visible = true;
            if (updatelist.Count > 0)
            {
                var url = String.Format(updatelist[0], channel);
                updatelist.RemoveAt(0);
                var cule = new CheckUpdateListExecutor(url, this);
                cule.Finished += cule_Finished;
                var thread = new Thread(new ThreadStart(cule.Execute));
                WriteLog(String.Format("GetUpdateList:{0}", cule.Url));
                thread.Start();
            }
            else
            {
                WriteErrorLog(updatelistfail);
                ShowMessageBox(updatelistfail);
            }
        }

        void cule_Finished(object sender, EventArgs e)
        {
            var cule = sender as CheckUpdateListExecutor;
            if (cule.Success)
            {
                label3.Visible = false;
                pictureBox1.Visible = false;
                foreach (UpdateInfo ui in cule.UpdateInfos)
                {
                    if (ui.VersionInfo.CompareTo(installInfo.InstallVersion) > 0)
                    {
                        if (ui.AssemblyType == AssemblyType.x64)
                        {
                            dataGridView1.Rows.Add(ui, waiting);
                        }

                    }
                }
                if (dataGridView1.RowCount == 0)
                {
                    ShowMessageBox(alreadylatest);
                    WriteLog("updatecheck:latest");
                    if (WorkState == WorkState.DownLoad)
                    {
                        this.Close();
                    }
                }
                else
                {
                    ProcessDownload();
                }
            }
            else
            {
                WriteErrorLog(String.Format("GetUpdateList:{0}", cule.ErrorLog));
                ProcessDownloadAndInstall();
            }
        }

        private void ReadInstallInfo()
        {
            if (File.Exists(installinfofile))
            {
                var sr = new StreamReader(installinfofile);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                installInfo.PPD = setting["PPD"] == "1";
                installInfo.SharpDX = setting["SharpDX"] == "1";
                installInfo.DirectShowLib = setting["DirectShowLib"] == "1";
                installInfo.IPAFont = setting["IPAFont"] == "1";
                installInfo.PPDeditor = setting["PPDeditor"] == "1";
                installInfo.BMSTOPPD = setting["BMSTOPPD"] == "1";
                installInfo.ffdshow = setting["ffdshow"] == "1";
                installInfo.MP4Splitter = setting["MP4Splitter"] == "1";
                installInfo.FLVSplitter = setting["FLVSplitter"] == "1";
                installInfo.Effect2DEditor = setting["Effect2DEditor"] == "1";
                installInfo.PPDVersion = new VersionInfo(setting["PPDVersion"]);
                installInfo.PPDeditorVersion = new VersionInfo(setting["PPDeditorVersion"]);
                installInfo.BMSTOPPDVersion = new VersionInfo(setting["BMSTOPPDVersion"]);
                installInfo.Effect2DEditorVersion = new VersionInfo(setting["Effect2DEditorVersion"]);
                installInfo.InstallVersion = new VersionInfo(setting["InstallVersion"]);

                label2.Text = installInfo.InstallVersion.ToString();
            }
        }

        private void ReadSetting()
        {
            if (File.Exists(iniFileName))
            {
                var sr = new StreamReader(iniFileName);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                string urls = setting["UrlList"];
                updatelist.AddRange(urls.Split('\n'));
                channel = setting["Channel"];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckFinish() || ShowMessageBox(askfinish, askfinishcaption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private bool CheckFinish()
        {
            bool ret = true;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if ((string)dataGridView1[State.Name, i].Value != installfinished)
                {
                    return false;
                }
            }
            return ret;
        }

        private void WriteLog(string log)
        {
            textBox1.AppendText(log);
            textBox1.AppendText("\r\n");
        }

        private void WriteErrorLog(string log)
        {
            OpenLog();
            textBox1.AppendText("Error: ");
            textBox1.AppendText(log);
            textBox1.AppendText("\r\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!IsLogOpen)
            {
                OpenLog();
            }
            else
            {
                groupBox1.Height = 15;
                this.ClientSize = new Size(this.ClientSize.Width, groupBox1.Location.Y + groupBox1.Height + 50);
            }
        }

        private void OpenLog()
        {
            if (!IsLogOpen)
            {
                groupBox1.Height = 120;
                this.ClientSize = new Size(this.ClientSize.Width, groupBox1.Location.Y + groupBox1.Height + 50);
            }
        }

        private bool IsLogOpen
        {
            get
            {
                return !(groupBox1.Height < 50);
            }
        }

        public void ShowMessageBox(string message)
        {
            ShowMessageBox(message, "", MessageBoxButtons.OK, DialogResult.OK);
        }

        public DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons buttons, DialogResult whennotvisible)
        {
            if (this.Visible && WorkState == WorkState.DownloadAndInstall)
            {
                return MessageBox.Show(message, caption, buttons);
            }
            else
            {
                return whennotvisible;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(logfile))
            {
                var fi = new FileInfo(logfile);
                if (fi.Length >= 1000000)
                {
                    File.Delete(logfile);
                }
            }
            using (StreamWriter sw = new StreamWriter(logfile, true))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(this.textBox1.Text);
            }

            var sr = new StreamReader(iniFileName);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            setting.ReplaceOrAdd("Language", langFileISO);
            using (SettingWriter sw = new SettingWriter(iniFileName, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
        }

        private void 終了ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckFinish() || ShowMessageBox(askfinish, askfinishcaption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
