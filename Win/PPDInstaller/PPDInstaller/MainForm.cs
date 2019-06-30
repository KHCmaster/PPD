using PPDConfiguration;
using PPDInstaller.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PPDInstaller
{
    public partial class MainForm : Form, IPanelManager
    {
        bool interrupted;
        bool ifinstall;

        const string DataFilePath = "Data.pak";

        string askfinishtext = "インストールを終了してもよろしいですか？";
        string askfinishcaption = "終了確認";

        string cachefolder = "Cache\\";

        string NoDataFile = "Dataファイルが存在しないのでインストールを行うことができません。";

        string config = "コンフィグ";
        string keyconfig = "キーコンフィグ";
        string createthumb = "サムネ作成";
        string songsfolder = "譜面フォルダ";

        string langFileISO = "jp";
        string fontName = "IPAGothic";

        private List<PanelBase> panels;
        private int currentIndex;

        public string DataDirectory
        {
            get;
            private set;
        }

        public PanelBase CurrentPanel
        {
            get { return panels[currentIndex]; }
        }

        public bool CanNext
        {
            get
            {
                return panels[currentIndex].CanNext;
            }
        }

        public bool CanPrevious
        {
            get
            {
                return panels[currentIndex].CanPrevious;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            panels = new List<PanelBase>();

            if (!Directory.Exists(cachefolder))
            {
                Directory.CreateDirectory(cachefolder);
            }
            //check lang
            if (Directory.Exists("Lang") && File.Exists("Lang\\LangList.ini"))
            {
                var sr = new StreamReader("Lang\\LangList.ini");
                var filenames = new List<string>();
                while (!sr.EndOfStream)
                {
                    filenames.Add(sr.ReadLine());
                }
                sr.Close();
                var fns = filenames.ToArray();
                var fm2 = new LangSelectForm();
                fm2.SetLang(fns);
                fm2.ShowDialog();
                if (File.Exists(String.Format("Lang\\{0}.ini", fm2.SelectedIndex)))
                {
                    ReadLang(String.Format("Lang\\{0}.ini", fm2.SelectedIndex));
                }
            }

            if (!File.Exists(DataFilePath))
            {
                MessageBox.Show(NoDataFile);
                Environment.Exit(-1);
                return;
            }

            var path = Path.GetRandomFileName();
            var extractDialog = new ExtractDialog
            {
                ExtractDir = Path.Combine(Path.GetTempPath(), path)
            };
            Directory.CreateDirectory(extractDialog.ExtractDir);
            extractDialog.UnzipFilePath = DataFilePath;
            extractDialog.ShowDialog();
            DataDirectory = Path.Combine(extractDialog.ExtractDir, "Data");

            confirmPanel1.InstallFinished += confirmPanel1_InstallFinished;

            AddPanel(this.startPanel1);
            AddPanel(this.installPanel1);
            AddPanel(this.installComponentPanel1);
            AddPanel(this.linkCreatePanel1);
            AddPanel(this.confirmPanel1);
            AddPanel(this.finishPanel1);
            AddPanel(this.installAbortedPanel1);
            panels[currentIndex].OnShown(false);
            ApplyPanelProperty(panels[currentIndex]);
        }

        private void AddPanel(PanelBase panelBase)
        {
            panels.Add(panelBase);
            panelBase.PanelManager = this;
            this.splitContainer2.Panel1.Controls.Add(panelBase);
        }

        void ReadLang(string filename)
        {
            if (!File.Exists(filename)) return;
            var sr = new StreamReader(filename);
            var s = sr.ReadToEnd();
            sr.Close();
            var lang = new SettingReader(s);
            askfinishtext = lang["AskFinishText"];
            askfinishcaption = lang["AskFinishCaption"];
            this.cancelButton.Text = lang["Button1"];
            this.nextButton.Text = lang["Button2"];
            this.previousButton.Text = lang["Button3"];
            NoDataFile = lang["NonExistDataFile"];
            config = lang["Config"];
            keyconfig = lang["KeyConfig"];
            createthumb = lang["CreateThumb"];
            songsfolder = lang["ScoreFolder"];
            langFileISO = lang["Language"];
            fontName = lang["FontName"];
            confirmPanel1.SetLang(lang);
            finishPanel1.SetLang(lang);
            installAbortedPanel1.SetLang(lang);
            installComponentPanel1.SetLang(lang);
            installPanel1.SetLang(lang);
            linkCreatePanel1.SetLang(lang);
            startPanel1.SetLang(lang);
        }

        public T GetPanel<T>() where T : PanelBase
        {
            return panels.Find(p => p is T) as T;
        }

        public void Next()
        {
            PanelBase currentPanel = panels[currentIndex];
            var nextPanel = panels[currentIndex + 1] as PanelBase;

            if (currentPanel is ConfirmPanel && !confirmPanel1.IsInstallFinishsd)
            {
                ifinstall = true;
                previousButton.Enabled = nextButton.Enabled = cancelButton.Enabled = false;
                confirmPanel1.Install();
            }
            else
            {
                currentPanel.Visible = false;
                nextPanel.Visible = true;
                currentIndex++;
                panels[currentIndex].OnShown(false);
                ApplyPanelProperty(panels[currentIndex]);
            }
        }

        public void Previous()
        {
            PanelBase currentPanel = panels[currentIndex];
            var previousPanel = panels[currentIndex - 1] as PanelBase;

            previousPanel.Visible = true;
            currentPanel.Visible = false;
            currentIndex--;
            panels[currentIndex].OnShown(false);
            ApplyPanelProperty(panels[currentIndex]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ifinstall)
            {
                e.Cancel = true;
                return;
            }
            if (!interrupted)
            {
                e.Cancel = true;
                if (MessageBox.Show(askfinishtext, askfinishcaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    interrupted = true;
                    foreach (var panel in panels)
                    {
                        panel.Visible = false;
                    }
                    currentIndex = panels.Count - 1;
                    panels[currentIndex].Visible = true;
                    ApplyPanelProperty(panels[currentIndex]);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (interrupted)
            {
                this.Close();
                return;
            }
            Next();
        }

        private void ApplyPanelProperty(PanelBase panel)
        {
            nextButton.Enabled = panel.CanNext;
            nextButton.Visible = panel.IsNextVisible;
            nextButton.Text = panel.NextText;
            previousButton.Enabled = panel.CanPrevious;
            previousButton.Visible = panel.IsPreviousVisible;
            previousButton.Text = panel.PreviousText;
            cancelButton.Enabled = panel.CanCancel;
            cancelButton.Visible = panel.IsCancelVisible;
            cancelButton.Text = panel.CancelText;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Previous();
        }

        void confirmPanel1_InstallFinished(object sender, EventArgs e)
        {
            confirmPanel1.InstallInfo.RegisterToMenu = linkCreatePanel1.CreateStartMenu;
            WriteLangISO();
            //WriteInstallInfo
            WriteInstallData();
            //RegisterStartMenu
            RegisterStartMenu();
            //Finish
            finishPanel1.ErrorText = confirmPanel1.ErrorText;
            ifinstall = false;
            interrupted = true;
            Next();
        }

        void WriteInstallData()
        {
            var path = Path.Combine(installPanel1.InstallDirectory, "PPD\\install.info");
            var installedInfo = new Dictionary<string, bool>();
            foreach (string key in InstallInfo.InstallationInfoData)
            {
                installedInfo.Add(key, false);
            }
            try
            {
                if (File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        var setting = new SettingReader(reader.ReadToEnd());
                        foreach (string key in InstallInfo.InstallationInfoData)
                        {
                            installedInfo[key] |= setting[key] == "1";
                        }
                    }
                }

                foreach (string key in InstallInfo.InstallationInfoData)
                {
                    installedInfo[key] |= confirmPanel1.InstallInfo.GetInstalled(key);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            try
            {
                var sw = new SettingWriter(path, false);
                foreach (KeyValuePair<string, bool> kvp in installedInfo)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
                if (installComponentPanel1.PPDVersion != "")
                {
                    sw.Write("PPDVersion", installComponentPanel1.PPDVersion);
                }
                if (installComponentPanel1.BMSTOPPDVersion != "")
                {
                    sw.Write("BMSTOPPDVersion", installComponentPanel1.BMSTOPPDVersion);
                }
                if (installComponentPanel1.Effect2DEditorVersion != "")
                {
                    sw.Write("Effect2DEditorVersion", installComponentPanel1.Effect2DEditorVersion);
                }
                sw.Write("InstallVersion", installComponentPanel1.InstallVersion);
                sw.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        void WriteLangISO()
        {
            if (confirmPanel1.InstallInfo.PPD)
            {
                WriteFontName(Path.Combine(installPanel1.InstallDirectory, "PPD\\PPD.ini"));
                WriteLangISO(Path.Combine(installPanel1.InstallDirectory, "PPD\\PPD.ini"));
                WriteLangISO(Path.Combine(installPanel1.InstallDirectory, "PPD\\PPDUpdater.ini"));
                WriteLangISO(Path.Combine(installPanel1.InstallDirectory, "PPD\\PPDConfig.ini"));
                WriteLangISO(Path.Combine(installPanel1.InstallDirectory, "PPD\\KeyConfiger.ini"));
                WriteLangISO(Path.Combine(installPanel1.InstallDirectory, "PPD\\PPDEditor.ini"));
                WriteLangISO(Path.Combine(installPanel1.InstallDirectory, "PPD\\Effect2DEditor.ini"));
            }
        }

        void WriteLangISO(string path)
        {
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                setting.ReplaceOrAdd("Language", langFileISO);
                using (SettingWriter sw = new SettingWriter(path, false))
                {
                    foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                    {
                        sw.Write(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        void WriteFontName(string path)
        {
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                setting.ReplaceOrAdd("fontname", fontName);
                using (SettingWriter sw = new SettingWriter(path, false))
                {
                    foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                    {
                        sw.Write(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        void RegisterStartMenu()
        {
            try
            {
                if (confirmPanel1.InstallInfo.RegisterToMenu)
                {
                    var programs = Win32API.GetCommonStartmenuPath();
                    var main = Path.Combine(programs, "PPD");
                    CreateDirectory(main);
                    if (confirmPanel1.InstallInfo.PPD)
                    {
                        //PPD
                        var ppddir = Path.Combine(main, "PPD");
                        CreateDirectory(ppddir);
                        CreateShortCut(Path.Combine(ppddir, "PPD.lnk"), Path.Combine(installPanel1.InstallDirectory, "PPD\\PPD.exe"), "", Path.Combine(installPanel1.InstallDirectory, "PPD"));
                        CreateShortCut(Path.Combine(ppddir, config + ".lnk"), Path.Combine(installPanel1.InstallDirectory, "PPD\\PPDConfig.exe"), "", Path.Combine(installPanel1.InstallDirectory, "PPD"));
                        CreateShortCut(Path.Combine(ppddir, keyconfig + ".lnk"), Path.Combine(installPanel1.InstallDirectory, "PPD\\KeyConfiger.exe"), "", Path.Combine(installPanel1.InstallDirectory, "PPD"));
                        CreateShortCut(Path.Combine(ppddir, "PPDeditor.lnk"), Path.Combine(installPanel1.InstallDirectory, "PPD\\PPDeditor.exe"), "", Path.Combine(installPanel1.InstallDirectory, "PPDeditor"));
                        CreateShortCut(Path.Combine(ppddir, songsfolder + ".lnk"), "explorer.exe", @"/n," + Path.Combine(installPanel1.InstallDirectory, "PPD\\songs"), "");
                    }
                    if (confirmPanel1.InstallInfo.BMSTOPPD)
                    {
                        //BMSTOPPD
                        var bmstoppddir = Path.Combine(main, "BMSTOPPD");
                        CreateDirectory(bmstoppddir);
                        CreateShortCut(Path.Combine(bmstoppddir, "BMSTOPPD.lnk"), Path.Combine(installPanel1.InstallDirectory, "BMSTOPPD\\BMSTOPPD.exe"), "", Path.Combine(installPanel1.InstallDirectory, "BMSTOPPD"));
                    }
                }
            }
            catch
            {
            }
        }

        void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        void CreateShortCut(string path, string targetPath, string Arguments, string workingDirectory)
        {
            var wsc = new IWshRuntimeLibrary.WshShellClass();

            var ws = (IWshRuntimeLibrary.WshShortcut)(wsc.CreateShortcut(path));
            ws.TargetPath = targetPath;// @"C:\Windows\Notepad.exe";
            ws.IconLocation = ws.TargetPath + ",0";
            ws.Arguments = Arguments;// @"""C:\Temp\Test.txt""";
            ws.WorkingDirectory = workingDirectory;// @"C:\Temp";

            // ショートカットの保存
            ws.Save();
        }

    }
}
