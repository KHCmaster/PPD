using PPDUpdater.Executor;
using PPDUpdater.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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

    /// <summary>
    /// アップデートの確認、ダウンロード、インストールを行うウィンドウです。
    /// </summary>
    /// <remarks>
    /// 引数（-d、-i）でダウンロードだけ・インストールだけを指定したときは、ウィンドウを表示せずに処理して終了する。
    /// </remarks>
    public partial class MainWindow : Window
    {
        const string installinfofile = "install.info";
        const string iniFileName = "PPDUpdater.ini";
        const string logfile = "update.log";

        string askfinish = "未インストールのバージョンがありますが、終了してもよろしいですか？";
        string askfinishcaption = "終了確認";
        string alreadylatest = "既に最新版です";
        string updatelistfail = "アップデートリストの取得に失敗しました";

        string waiting = "待機中";
        string downloading = "ダウンロード中";
        string downloadfinished = "ダウンロード完了";
        string installing = "インストール中";
        string installfinished = "インストール完了";

        string updatefinished = "アップデートが正常に終了しました";

        readonly InstallInfo installInfo = new();
        readonly List<string> updatelist = [];
        readonly ObservableCollection<UpdateRow> rows = [];
        string channel;

        private string langFileName = "";
        private string langFileISO = "";

        public WorkState WorkState
        {
            get;
            set;
        }

        public MainWindow()
        {
            InitializeComponent();
            updateGrid.ItemsSource = rows;

            WorkState = WorkState.DownloadAndInstall;

            ReadInstallInfo();
            ReadSetting();

            if (!Directory.Exists(Utility.DownloadDirectory))
            {
                Directory.CreateDirectory(Utility.DownloadDirectory);
            }

            CheckSetting();
            CheckLangFiles();
            SetLanguage(langFileName);
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var setting = new SettingReader(File.ReadAllText(iniFileName));
                langFileISO = setting["Language"];
                langFileName = Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO));
            }
        }

        private void CheckLangFiles()
        {
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            var componentName = this.GetType().Assembly.GetName().Name;
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
            {
                var setting = new SettingReader(File.ReadAllText(fileName));
                var menuItem = new MenuItem
                {
                    // 名前の「_」がアクセスキーとして扱われないように、TextBlock で表示する
                    Header = new TextBlock { Text = setting["DisplayName"] },
                    Tag = fileName,
                    IsChecked = Path.GetFileName(fileName).Equals(String.Format("lang_{0}_{1}.ini", componentName, langFileISO), StringComparison.CurrentCultureIgnoreCase)
                };
                menuItem.Click += LanguageMenuItem_Click;
                languageMenuItem.Items.Add(menuItem);
            }
        }

        void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            langFileName = (string)menuItem.Tag;
            var m = LangFileRegex().Match(Path.GetFileName(langFileName));
            if (m.Success)
            {
                langFileISO = m.Groups["ISO"].Value;
            }
            SetLanguage(langFileName);
            foreach (var child in languageMenuItem.Items)
            {
                if (child is MenuItem item)
                {
                    item.IsChecked = false;
                }
            }
            menuItem.IsChecked = true;
        }

        private void SetLanguage(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            var setting = new SettingReader(File.ReadAllText(fileName));
            fileMenuItem.Header = setting["File"];
            exitMenuItem.Header = setting["Finish"];
            languageMenuItem.Header = setting["Language"];
            askfinish = setting["AskFinish"];
            askfinishcaption = setting["AskFinishCaption"];
            alreadylatest = setting["AlreadyLatest"];
            updatelistfail = setting["UpdateListFail"];
            waiting = setting["Waiting"];
            downloading = setting["Downloading"];
            downloadfinished = setting["DownloadFinished"];
            installing = setting["Installing"];
            installfinished = setting["InstallFinished"];
            updatefinished = setting["UpdateFinished"];
            versionColumn.Header = setting["Header1"];
            stateColumn.Header = setting["Header2"];
            downloadColumn.Header = setting["Download"];
            installColumn.Header = setting["Install"];
            currentVersionLabel.Text = setting["Label1"];
            checkingText.Text = setting["Label3"];
            logGroupBox.Header = setting["GroupBox1"];
            closeButton.Content = setting["Button1"];
            toggleLogButton.Content = setting["Button2"];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WorkState == WorkState.DownloadAndInstall)
            {
                ShowInTaskbar = true;
                WindowState = WindowState.Normal;
                Activate();
            }
            else
            {
                Hide();
            }

            // 表示の処理が終わってから始める（処理の中でウィンドウを閉じることがあるため）
            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                if (WorkState == WorkState.DownLoad || WorkState == WorkState.DownloadAndInstall)
                {
                    ProcessDownloadAndInstall();
                }
                else if (WorkState == WorkState.Install)
                {
                    FindDownloadData();
                }
            });
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
                                rows.Add(new UpdateRow(ui, downloadfinished));
                            }
                        }
                    }
                }
                ProcessInstall();
            }
            else
            {
                WriteLog("install:no update");
                Close();
            }
        }

        private void ProcessDownload()
        {
            bool downloaded = false;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].State == waiting)
                {
                    downloaded = true;
                    rows[i].State = downloading;
                    var due = new DownloadUpdateExecutor(rows[i].Info, Dispatcher, i);
                    due.Progressed += Due_Progressed;
                    due.Finished += Due_Finished;
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
                    Close();
                }
            }
        }

        void Due_Finished(object sender, EventArgs e)
        {
            var due = sender as DownloadUpdateExecutor;
            rows[due.Index].DownloadProgress = 100;
            if (due.Success)
            {
                rows[due.Index].State = downloadfinished;
                ProcessDownload();
            }
            else
            {
                WriteErrorLog(String.Format("DownloadUpdate:{0}", due.ErrorLog));
            }
        }

        void Due_Progressed(object sender, EventArgs e)
        {
            var due = sender as DownloadUpdateExecutor;
            rows[due.Index].DownloadProgress = due.Progress;
        }

        private void ProcessInstall()
        {
            bool installed = false;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].State == downloadfinished)
                {
                    installed = true;
                    rows[i].State = installing;
                    var ui = rows[i].Info;
                    var iue = new InstallUpdateExecutor(ui.FilePath, ui.VersionInfo, installInfo, i, Dispatcher);
                    iue.Progressed += Iue_Progressed;
                    iue.Finished += Iue_Finished;
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
                    Close();
                }
            }
        }

        void Iue_Progressed(object sender, EventArgs e)
        {
            var iue = sender as InstallUpdateExecutor;
            rows[iue.Index].InstallProgress = iue.Progress;
        }

        void Iue_Finished(object sender, EventArgs e)
        {
            var iue = sender as InstallUpdateExecutor;
            installInfo.InstallVersion = iue.InstallVersion;
            rows[iue.Index].InstallProgress = 100;
            if (iue.Success)
            {
                WriteInstallInfo();
                rows[iue.Index].State = installfinished;
                ProcessInstall();
            }
            else
            {
                WriteErrorLog(String.Format("InstallUpdate:{0}", iue.ErrorLog));
            }
        }

        private void WriteInstallInfo()
        {
            using StreamWriter sw = new(installinfofile);
            WriteData(sw, "PPD", installInfo.PPD);
            WriteData(sw, "SharpDX", installInfo.SharpDX);
            WriteData(sw, "DirectShowLib", installInfo.DirectShowLib);
            WriteData(sw, "IPAFont", installInfo.IPAFont);
            WriteData(sw, "PPDeditor", installInfo.PPDeditor);
            WriteData(sw, "ffdshow", installInfo.Ffdshow);
            WriteData(sw, "MP4Splitter", installInfo.MP4Splitter);
            WriteData(sw, "FLVSplitter", installInfo.FLVSplitter);
            WriteData(sw, "Effect2DEditor", installInfo.Effect2DEditor);
            WriteData(sw, "PPDVersion", installInfo.PPDVersion.ToString());
            WriteData(sw, "PPDeditorVersion", installInfo.PPDeditorVersion.ToString());
            WriteData(sw, "BMSTOPPDVersion", installInfo.BMSTOPPDVersion.ToString());
            WriteData(sw, "Effect2DEditorVersion", installInfo.Effect2DEditorVersion.ToString());
            WriteData(sw, "InstallVersion", installInfo.InstallVersion.ToString());
        }

        private static void WriteData(StreamWriter sw, string key, bool value)
        {
            WriteData(sw, key, value ? "1" : "0");
        }

        private static void WriteData(StreamWriter sw, string key, string value)
        {
            sw.Write(String.Format("[{0}]{1}", key, value));
            sw.WriteLine();
        }

        private void ProcessDownloadAndInstall()
        {
            checkingPanel.Visibility = Visibility.Visible;
            if (updatelist.Count > 0)
            {
                var url = String.Format(updatelist[0], channel);
                updatelist.RemoveAt(0);
                var cule = new CheckUpdateListExecutor(url, Dispatcher);
                cule.Finished += Cule_Finished;
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

        void Cule_Finished(object sender, EventArgs e)
        {
            var cule = sender as CheckUpdateListExecutor;
            if (cule.Success)
            {
                checkingPanel.Visibility = Visibility.Hidden;
                foreach (UpdateInfo ui in cule.UpdateInfos)
                {
                    if (ui.VersionInfo.CompareTo(installInfo.InstallVersion) > 0 && ui.AssemblyType == AssemblyType.x64)
                    {
                        rows.Add(new UpdateRow(ui, waiting));
                    }
                }
                if (rows.Count == 0)
                {
                    ShowMessageBox(alreadylatest);
                    WriteLog("updatecheck:latest");
                    if (WorkState == WorkState.DownLoad)
                    {
                        Close();
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
            if (!File.Exists(installinfofile))
            {
                return;
            }
            var setting = new SettingReader(File.ReadAllText(installinfofile));
            installInfo.PPD = setting["PPD"] == "1";
            installInfo.SharpDX = setting["SharpDX"] == "1";
            installInfo.DirectShowLib = setting["DirectShowLib"] == "1";
            installInfo.IPAFont = setting["IPAFont"] == "1";
            installInfo.PPDeditor = setting["PPDeditor"] == "1";
            installInfo.BMSTOPPD = setting["BMSTOPPD"] == "1";
            installInfo.Ffdshow = setting["ffdshow"] == "1";
            installInfo.MP4Splitter = setting["MP4Splitter"] == "1";
            installInfo.FLVSplitter = setting["FLVSplitter"] == "1";
            installInfo.Effect2DEditor = setting["Effect2DEditor"] == "1";
            installInfo.PPDVersion = new VersionInfo(setting["PPDVersion"]);
            installInfo.PPDeditorVersion = new VersionInfo(setting["PPDeditorVersion"]);
            installInfo.BMSTOPPDVersion = new VersionInfo(setting["BMSTOPPDVersion"]);
            installInfo.Effect2DEditorVersion = new VersionInfo(setting["Effect2DEditorVersion"]);
            installInfo.InstallVersion = new VersionInfo(setting["InstallVersion"]);

            currentVersionText.Text = installInfo.InstallVersion.ToString();
        }

        private void ReadSetting()
        {
            if (!File.Exists(iniFileName))
            {
                return;
            }
            var setting = new SettingReader(File.ReadAllText(iniFileName));
            updatelist.AddRange(setting["UrlList"].Split('\n'));
            channel = setting["Channel"];
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseIfConfirmed();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseIfConfirmed();
        }

        private void CloseIfConfirmed()
        {
            if (CheckFinish() || ShowMessageBox(askfinish, askfinishcaption, MessageBoxButton.YesNo, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private bool CheckFinish()
        {
            foreach (var row in rows)
            {
                if (row.State != installfinished)
                {
                    return false;
                }
            }
            return true;
        }

        private void WriteLog(string log)
        {
            logTextBox.AppendText(log);
            logTextBox.AppendText("\r\n");
        }

        private void WriteErrorLog(string log)
        {
            OpenLog();
            logTextBox.AppendText("Error: ");
            logTextBox.AppendText(log);
            logTextBox.AppendText("\r\n");
        }

        private void ToggleLogButton_Click(object sender, RoutedEventArgs e)
        {
            logTextBox.Visibility = IsLogOpen ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OpenLog()
        {
            logTextBox.Visibility = Visibility.Visible;
        }

        private bool IsLogOpen
        {
            get
            {
                return logTextBox.Visibility == Visibility.Visible;
            }
        }

        public void ShowMessageBox(string message)
        {
            ShowMessageBox(message, "", MessageBoxButton.OK, MessageBoxResult.OK);
        }

        // ウィンドウを表示していないとき（ダウンロードだけ・インストールだけのとき）は、メッセージを出さずに既定の答えを返す
        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxResult whennotvisible)
        {
            if (IsVisible && WorkState == WorkState.DownloadAndInstall)
            {
                return MessageBox.Show(this, message, caption, buttons);
            }
            return whennotvisible;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (File.Exists(logfile) && new FileInfo(logfile).Length >= 1000000)
            {
                File.Delete(logfile);
            }
            using (StreamWriter sw = new(logfile, true))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(logTextBox.Text);
            }

            var setting = new SettingReader(File.Exists(iniFileName) ? File.ReadAllText(iniFileName) : "");
            setting.ReplaceOrAdd("Language", langFileISO);
            using SettingWriter sw2 = new(iniFileName, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw2.Write(kvp.Key, kvp.Value);
            }
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();

        /// <summary>
        /// アップデートの一覧の 1 行です。
        /// </summary>
        class UpdateRow(UpdateInfo info, string state) : INotifyPropertyChanged
        {
            private string state = state;
            private int downloadProgress;
            private int installProgress;

            public event PropertyChangedEventHandler PropertyChanged;

            public UpdateInfo Info { get; } = info;

            public string State
            {
                get { return state; }
                set { state = value; OnPropertyChanged(); }
            }

            public int DownloadProgress
            {
                get { return downloadProgress; }
                set { downloadProgress = value; OnPropertyChanged(); }
            }

            public int InstallProgress
            {
                get { return installProgress; }
                set { installProgress = value; OnPropertyChanged(); }
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
