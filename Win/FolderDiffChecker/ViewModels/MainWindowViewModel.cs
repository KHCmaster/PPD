using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FolderDiffChecker.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FolderDiffChecker.ViewModels
{
    /// <summary>
    /// ViewModel から使う、ウィンドウの機能です。
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// フォルダーを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string SelectFolder(string initialDirectory);

        /// <summary>
        /// アップロードに使うユーザー名を入力させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskUserName(string lastUserName);

        void ShowMessage(string text);
    }

    /// <summary>
    /// 差分の一覧の 1 行です。
    /// </summary>
    public class DiffRow(DiffModel model, string name)
    {
        public DiffModel Model { get; } = model;

        public string Name { get; } = name;
    }

    /// <summary>
    /// 2 つのリリースフォルダーの差分を調べ、差分の配布物を作ってアップロードする画面です。
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        readonly IDialogService dialog;
        string outPath;
        string lastFolder;
        long startTime;

        [ObservableProperty]
        private string oldFolder = "";

        [ObservableProperty]
        private string newFolder = "";

        /// <summary>
        /// 強制的に変更済みにするファイル（「:」区切り）。
        /// </summary>
        [ObservableProperty]
        private string forceIncludeFiles = "";

        /// <summary>
        /// 一覧に出さないファイル名（「:」区切り）。
        /// </summary>
        [ObservableProperty]
        private string ignoreFiles = "PPD.ini:Data.pak";

        [ObservableProperty]
        private string statusText = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DiffCheckCommand), nameof(CopyDiffCommand), nameof(CopyDiffCleverCommand))]
        private bool isChecking;

        public MainWindowViewModel(IDialogService dialog, string releasesDir)
        {
            this.dialog = dialog;
            outPath = lastFolder = releasesDir;
            var pair = ReleaseFolder.FindLatestPair(releasesDir);
            if (pair.HasValue)
            {
                (OldFolder, NewFolder) = pair.Value;
            }
        }

        public ObservableCollection<DiffRow> Rows { get; } = [];

        private bool CanRunWithDiff()
        {
            return !IsChecking;
        }

        // フォルダーを選ぶ。前回選んだフォルダーから始める
        private string SelectFolder()
        {
            var folder = dialog.SelectFolder(lastFolder);
            if (folder != null)
            {
                lastFolder = folder;
            }
            return folder;
        }

        [RelayCommand]
        private void SelectOldFolder()
        {
            OldFolder = SelectFolder() ?? OldFolder;
        }

        [RelayCommand]
        private void SelectNewFolder()
        {
            NewFolder = SelectFolder() ?? NewFolder;
        }

        [RelayCommand(CanExecute = nameof(CanRunWithDiff))]
        private void DiffCheck()
        {
            if (!Directory.Exists(OldFolder) || !Directory.Exists(NewFolder))
            {
                dialog.ShowMessage("存在しないディレクトリを比較しています。");
                return;
            }

            IsChecking = true;
            Rows.Clear();

            var df = new DiffFinder(OldFolder, NewFolder,
                [
                    ThumbsDbRegex(),
                    PPDUpdaterExeRegex(),
                    PPDInstallerExeRegex()
                ],
                [.. ForceIncludeFiles.Split([":"], StringSplitOptions.RemoveEmptyEntries).Select(s => new Regex(Regex.Escape(s), RegexOptions.IgnoreCase))],
                SynchronizationContext.Current);
            df.Finished += Df_Finished;
            df.DiffFound += Df_DiffFound;
            df.BeforeCompare += text => StatusText = text;
            new Thread(df.Execute).Start();
            startTime = Environment.TickCount;
        }

        private void Df_Finished(object sender, EventArgs e)
        {
            Console.WriteLine("Finished:{0}", Environment.TickCount - startTime);
            IsChecking = false;
            StatusText = "";
        }

        private void Df_DiffFound(DiffModel diffModel)
        {
            var fileName = Path.GetFileName(diffModel.FilePath);
            if (diffModel.IsFile && Array.IndexOf(IgnoreFiles.Split(':'), fileName) >= 0)
            {
                return;
            }
            Rows.Add(new DiffRow(diffModel, fileName));
        }

        [RelayCommand(CanExecute = nameof(CanRunWithDiff))]
        private void CopyDiff()
        {
            var folder = SelectFolder();
            if (folder != null)
            {
                ReleaseFolder.CopyDiff(NewFolder, Rows.Select(r => r.Model), folder);
            }
        }

        [RelayCommand(CanExecute = nameof(CanRunWithDiff))]
        private void CopyDiffClever()
        {
            ReleaseFolder.CopyDiffToVersionFolder(NewFolder, Rows.Select(r => r.Model), outPath);
        }

        [RelayCommand]
        private void ZipAll()
        {
            ReleaseFolder.ZipAll(NewFolder);
        }

        [RelayCommand]
        private void UpdateList()
        {
            ReleaseFolder.UpdateList(OldFolder, NewFolder);
        }

        [RelayCommand]
        private void UploadBeta()
        {
            Upload((uploader, version, dottedVersion) => uploader.UploadBeta(outPath, version, dottedVersion));
        }

        [RelayCommand]
        private void Upload()
        {
            Upload((uploader, version, dottedVersion) => uploader.Upload(outPath, version, dottedVersion));
        }

        [RelayCommand]
        private void SessionTest()
        {
            Connect()?.Dispose();
        }

        private void Upload(Action<Uploader, string, string> action)
        {
            var version = ReleaseFolder.GetVersion(NewFolder, "");
            if (version == null)
            {
                return;
            }
            using var uploader = Connect();
            if (uploader == null)
            {
                return;
            }
            try
            {
                action(uploader, version, ReleaseFolder.GetVersion(NewFolder, "."));
            }
            catch (Exception ex)
            {
                dialog.ShowMessage(ex.ToString());
            }
        }

        private Uploader Connect()
        {
            var userName = dialog.AskUserName(Uploader.LastUserName);
            if (userName == null)
            {
                return null;
            }
            Uploader.LastUserName = userName;

            var uploader = Uploader.Connect(userName, out var error);
            if (uploader == null)
            {
                dialog.ShowMessage(error);
            }
            return uploader;
        }

        [GeneratedRegex("thumbs\\.db", RegexOptions.IgnoreCase)]
        private static partial Regex ThumbsDbRegex();
        [GeneratedRegex("PPDUpdater\\.exe", RegexOptions.IgnoreCase)]
        private static partial Regex PPDUpdaterExeRegex();
        [GeneratedRegex("PPDInstaller\\.exe", RegexOptions.IgnoreCase)]
        private static partial Regex PPDInstallerExeRegex();
    }
}
