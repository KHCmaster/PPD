using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDUpdater.Executor;
using PPDUpdater.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PPDUpdater.ViewModels
{
    /// <summary>
    /// ViewModel から使う、ウィンドウの機能です。
    /// </summary>
    public interface IWindowService
    {
        void ShowMessage(string message);

        /// <summary>
        /// はい・いいえで聞きます。
        /// </summary>
        bool Confirm(string message, string caption);

        void Close();
    }

    /// <summary>
    /// アップデートの確認、ダウンロード、インストールを行う画面です。
    /// </summary>
    /// <remarks>
    /// <see cref="WorkState"/> がダウンロードだけ・インストールだけのときは、ウィンドウを表示しないので、メッセージを出さずに既定の答えを使う。
    /// </remarks>
    public partial class MainWindowViewModel : ObservableObject
    {
        const string componentName = "PPDUpdater";

        readonly IWindowService window;
        readonly InstallInfo installInfo;
        readonly List<string> updatelist;
        readonly string channel;
        readonly StringBuilder log = new();
        string languageIso;

        [ObservableProperty]
        private bool isChecking;

        [ObservableProperty]
        private bool isLogVisible;

        public MainWindowViewModel(IWindowService window, WorkState workState)
        {
            this.window = window;
            WorkState = workState;

            installInfo = InstallInfoFile.Load();
            CurrentVersion = installInfo.InstallVersion.ToString();
            (updatelist, channel, languageIso) = UpdaterSetting.Load();

            if (!Directory.Exists(Utility.DownloadDirectory))
            {
                Directory.CreateDirectory(Utility.DownloadDirectory);
            }

            LoadLanguages();
            if (!String.IsNullOrEmpty(languageIso))
            {
                Texts.Load(Path.Combine("Lang", GetLanguageFileName(languageIso)));
            }
        }

        public WorkState WorkState { get; }

        /// <summary>
        /// ウィンドウを表示するかどうかです（ダウンロードもインストールもするときだけ表示する）。
        /// </summary>
        public bool IsInteractive
        {
            get { return WorkState == WorkState.DownloadAndInstall; }
        }

        public LanguageTexts Texts { get; } = new();

        public ObservableCollection<LanguageOption> Languages { get; } = [];

        public ObservableCollection<UpdateRow> Rows { get; } = [];

        public string CurrentVersion { get; }

        public string LogText
        {
            get { return log.ToString(); }
        }

        private static string GetLanguageFileName(string iso)
        {
            return String.Format("lang_{0}_{1}.ini", componentName, iso);
        }

        private void LoadLanguages()
        {
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
            {
                var setting = new SettingReader(File.ReadAllText(fileName));
                var m = LangFileRegex().Match(Path.GetFileName(fileName));
                var option = new LanguageOption(setting["DisplayName"], fileName, m.Success ? m.Groups["ISO"].Value : null)
                {
                    IsChecked = Path.GetFileName(fileName).Equals(GetLanguageFileName(languageIso), StringComparison.CurrentCultureIgnoreCase)
                };
                Languages.Add(option);
            }
        }

        [RelayCommand]
        private void SelectLanguage(LanguageOption option)
        {
            if (option.Iso != null)
            {
                languageIso = option.Iso;
            }
            Texts.Load(option.FileName);
            foreach (var language in Languages)
            {
                language.IsChecked = language == option;
            }
        }

        [RelayCommand]
        private void ToggleLog()
        {
            IsLogVisible = !IsLogVisible;
        }

        /// <summary>
        /// 未インストールのものがあれば確かめてから閉じます。
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            if (Rows.All(row => row.State == UpdateState.InstallFinished) || Confirm(Texts["AskFinish"], Texts["AskFinishCaption"], true))
            {
                window.Close();
            }
        }

        /// <summary>
        /// 処理を始めます。ウィンドウを表示してから呼びます（処理の中でウィンドウを閉じることがあるため）。
        /// </summary>
        public void Start()
        {
            if (WorkState == WorkState.DownLoad || WorkState == WorkState.DownloadAndInstall)
            {
                ProcessDownloadAndInstall();
            }
            else if (WorkState == WorkState.Install)
            {
                FindDownloadData();
            }
        }

        /// <summary>
        /// 閉じるときに、ログと言語の設定を保存します。
        /// </summary>
        public void SaveOnClosing()
        {
            UpdaterSetting.AppendLog(LogText);
            UpdaterSetting.SaveLanguage(languageIso);
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
                            var m = Utility.UpdateRegex.Match(Path.GetFileName(fn));
                            if (m.Success)
                            {
                                var ui = new UpdateInfo(m.Groups[Utility.UpdateRegexGroup].Value, "")
                                {
                                    FilePath = fn
                                };
                                Rows.Add(new UpdateRow(ui, UpdateState.DownloadFinished, Texts));
                            }
                        }
                    }
                }
                ProcessInstall();
            }
            else
            {
                WriteLog("install:no update");
                window.Close();
            }
        }

        private void ProcessDownloadAndInstall()
        {
            IsChecking = true;
            if (updatelist.Count > 0)
            {
                var url = String.Format(updatelist[0], channel);
                updatelist.RemoveAt(0);
                var cule = new CheckUpdateListExecutor(url, SynchronizationContext.Current);
                cule.Finished += Cule_Finished;
                WriteLog(String.Format("GetUpdateList:{0}", cule.Url));
                new Thread(cule.Execute).Start();
            }
            else
            {
                WriteErrorLog(Texts["UpdateListFail"]);
                ShowMessage(Texts["UpdateListFail"]);
            }
        }

        private void Cule_Finished(object sender, EventArgs e)
        {
            var cule = (CheckUpdateListExecutor)sender;
            if (!cule.Success)
            {
                WriteErrorLog(String.Format("GetUpdateList:{0}", cule.ErrorLog));
                ProcessDownloadAndInstall();
                return;
            }

            IsChecking = false;
            foreach (UpdateInfo ui in cule.UpdateInfos)
            {
                if (ui.VersionInfo.CompareTo(installInfo.InstallVersion) > 0 && ui.AssemblyType == AssemblyType.x64)
                {
                    Rows.Add(new UpdateRow(ui, UpdateState.Waiting, Texts));
                }
            }
            if (Rows.Count == 0)
            {
                ShowMessage(Texts["AlreadyLatest"]);
                WriteLog("updatecheck:latest");
                if (WorkState == WorkState.DownLoad)
                {
                    window.Close();
                }
            }
            else
            {
                ProcessDownload();
            }
        }

        private void ProcessDownload()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].State == UpdateState.Waiting)
                {
                    Rows[i].State = UpdateState.Downloading;
                    var due = new DownloadUpdateExecutor(Rows[i].Info, SynchronizationContext.Current, i);
                    due.Progressed += (sender, e) => Rows[due.Index].DownloadProgress = due.Progress;
                    due.Finished += Due_Finished;
                    WriteLog(String.Format("DownloadUpdate:{0}", due.URL));
                    new Thread(due.Execute).Start();
                    return;
                }
            }

            if (WorkState == WorkState.DownloadAndInstall)
            {
                ProcessInstall();
            }
            else
            {
                window.Close();
            }
        }

        private void Due_Finished(object sender, EventArgs e)
        {
            var due = (DownloadUpdateExecutor)sender;
            Rows[due.Index].DownloadProgress = 100;
            if (due.Success)
            {
                Rows[due.Index].State = UpdateState.DownloadFinished;
                ProcessDownload();
            }
            else
            {
                WriteErrorLog(String.Format("DownloadUpdate:{0}", due.ErrorLog));
            }
        }

        private void ProcessInstall()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].State == UpdateState.DownloadFinished)
                {
                    Rows[i].State = UpdateState.Installing;
                    var ui = Rows[i].Info;
                    var iue = new InstallUpdateExecutor(ui.FilePath, ui.VersionInfo, installInfo, i, SynchronizationContext.Current);
                    iue.Progressed += (sender, e) => Rows[iue.Index].InstallProgress = iue.Progress;
                    iue.Finished += Iue_Finished;
                    WriteLog(String.Format("InstallUpdate:{0}", iue.FilePath));
                    new Thread(iue.Execute).Start();
                    return;
                }
            }

            ShowMessage(Texts["UpdateFinished"]);
            if (WorkState == WorkState.Install)
            {
                window.Close();
            }
        }

        private void Iue_Finished(object sender, EventArgs e)
        {
            var iue = (InstallUpdateExecutor)sender;
            installInfo.InstallVersion = iue.InstallVersion;
            Rows[iue.Index].InstallProgress = 100;
            if (iue.Success)
            {
                InstallInfoFile.Save(installInfo);
                Rows[iue.Index].State = UpdateState.InstallFinished;
                ProcessInstall();
            }
            else
            {
                WriteErrorLog(String.Format("InstallUpdate:{0}", iue.ErrorLog));
            }
        }

        private void WriteLog(string text)
        {
            log.Append(text).Append("\r\n");
            OnPropertyChanged(nameof(LogText));
        }

        private void WriteErrorLog(string text)
        {
            IsLogVisible = true;
            WriteLog("Error: " + text);
        }

        private void ShowMessage(string message)
        {
            if (IsInteractive)
            {
                window.ShowMessage(message);
            }
        }

        private bool Confirm(string message, string caption, bool whenNotInteractive)
        {
            return IsInteractive ? window.Confirm(message, caption) : whenNotInteractive;
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
    }
}
