using PPDConfiguration;
using PPDInstaller;
using PPDInstaller.Executor;
using PPDInstaller.Models;
using PPDInstaller.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Xunit;

namespace UnitTests.PPDInstaller
{
    public class LanguageTextsTests
    {
        [Fact]
        public void WithoutFile_UsesJapanese()
        {
            var texts = new LanguageTexts();
            texts.Load("missing.ini");

            Assert.Equal("次へ", texts["Button2"]);
            Assert.Null(texts["NoSuchKey"]);
        }

        [Fact]
        public void LoadsLanguageFileAndFallsBackForMissingKeys()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            AppFiles.CopyTo("PPDInstaller", folder);
            var texts = new LanguageTexts();

            texts.Load(Path.Combine("Lang", "1.ini"));

            Assert.Equal("Next", texts["Button2"]);
            Assert.Equal("en", texts["Language"]);
            folder.Write("partial.ini", "[Button2]Weiter\n");
            texts.Load("partial.ini");
            Assert.Equal("Weiter", texts["Button2"]);
            Assert.Equal("キャンセル", texts["Button1"]);
        }
    }

    public class InstallFinisherTests
    {
        [Fact]
        public void WriteLanguage_ReplacesLanguageAndFontInExistingFiles()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("PPD", "PPD.ini"), "[fontname]Old\n[Language]jp\n[width]800\n");
            folder.Write(Path.Combine("PPD", "PPDConfig.ini"), "");
            var installDirectory = folder.Path + Path.DirectorySeparatorChar;

            InstallFinisher.WriteLanguage(installDirectory, new InstallInfo { PPD = true }, "en", "NewFont");

            var ppd = new SettingReader(File.ReadAllText(folder.Combine("PPD", "PPD.ini")));
            Assert.Equal(("NewFont", "en", "800"), (ppd["fontname"], ppd["Language"], ppd["width"]));
            Assert.Equal("en", new SettingReader(File.ReadAllText(folder.Combine("PPD", "PPDConfig.ini")))["Language"]);
            // ないファイルは作らない
            Assert.False(File.Exists(folder.Combine("PPD", "KeyConfiger.ini")));
        }

        [Fact]
        public void WriteLanguage_WithoutPPD_DoesNothing()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("PPD", "PPD.ini"), "[Language]jp\n");

            InstallFinisher.WriteLanguage(folder.Path, new InstallInfo(), "en", "Font");

            Assert.Equal("[Language]jp\n", File.ReadAllText(folder.Combine("PPD", "PPD.ini")));
        }

        [Fact]
        public void WriteInstallData_MergesWithPreviousInstall()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write(Path.Combine("PPD", "install.info"), "[PPD]0\n[BMSTOPPD]1\n");
            var errors = new List<string>();

            InstallFinisher.WriteInstallData(folder.Path, new InstallInfo { PPD = true }, InstallVersions.Read(), errors.Add);

            var info = new SettingReader(File.ReadAllText(folder.Combine("PPD", "install.info")));
            Assert.Equal(("1", "1", "0"), (info["PPD"], info["BMSTOPPD"], info["IPAFont"]));
            Assert.Equal(FileVersionInfo.GetVersionInfo(typeof(InstallInfo).Assembly.Location).FileVersion, info["InstallVersion"]);
            Assert.Empty(errors);
        }

        [Fact]
        public void WriteInstallData_WritesComponentVersions()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var exe = typeof(InstallInfo).Assembly.Location;
            var version = FileVersionInfo.GetVersionInfo(exe).FileVersion;
            foreach (var name in new[] { "PPD\\PPD.exe", "BMSTOPPD\\BMSTOPPD.exe", "Effect2DEditor\\Effect2DEditor.exe" })
            {
                Directory.CreateDirectory(Path.GetDirectoryName(folder.Combine("Data", name)));
                File.Copy(exe, folder.Combine("Data", name));
            }
            var install = folder.Combine("install");
            Directory.CreateDirectory(Path.Combine(install, "PPD"));

            InstallFinisher.WriteInstallData(install, new InstallInfo(), InstallVersions.Read(), _ => { });

            var info = new SettingReader(File.ReadAllText(Path.Combine(install, "PPD", "install.info")));
            Assert.Equal(version, info["PPDVersion"]);
            Assert.Equal(version, info["BMSTOPPDVersion"]);
            Assert.Equal(version, info["Effect2DEditorVersion"]);
        }

        [Fact]
        public void WriteInstallData_ReportsWriteError()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var errors = new List<string>();

            // PPD フォルダーがないので書き込めない
            InstallFinisher.WriteInstallData(folder.Path, new InstallInfo(), InstallVersions.Read(), errors.Add);

            Assert.Single(errors);
        }

        [Fact]
        public void RegisterStartMenu_NotRequested_DoesNothing()
        {
            // スタートメニューに書き込まないように、登録しない場合だけを確かめる
            InstallFinisher.RegisterStartMenu(@"C:\nowhere\", new InstallInfo { PPD = true, RegisterToMenu = false }, ("a", "b", "c"));
        }

        [Fact]
        public void ReadInstalled_ReadsVersions()
        {
            using var folder = new TempFolder();
            Assert.Null(InstallVersions.ReadInstalled(folder.Path));
            folder.Write(Path.Combine("PPD", "install.info"), "[PPDVersion] 1.2.3.4 \n[BMSTOPPDVersion]5.6.7.8\n");

            var installed = InstallVersions.ReadInstalled(folder.Path);

            Assert.Equal(("1.2.3.4", "5.6.7.8"), (installed.PPD, installed.BMSTOPPD));
        }

        [Fact]
        public void InstallInfo_GetInstalled()
        {
            var info = new InstallInfo { PPD = true, IPAFont = true, BMSTOPPD = false, LAVFilters = true, RegisterToMenu = true };

            Assert.Equal([true, true, false, true, true], InstallInfo.InstallationInfoData.Select(info.GetInstalled));
            Assert.False(info.GetInstalled("Unknown"));
        }
    }

    public partial class InstallExecutorTests
    {
        private static CExecutor Run(CExecutor executor)
        {
            var finished = false;
            executor.Finished += (sender, e) => finished = true;
            executor.Execute();
            Assert.True(finished);
            Assert.Equal("0", executor.Progress);
            return executor;
        }

        [Fact]
        public void PPD_CopiesFilesButKeepsExistingSettings()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("data", "PPD", "PPD.exe"), "new exe");
            folder.Write(Path.Combine("data", "PPD", "PPD.ini"), "[width]800\n");
            folder.Write(Path.Combine("data", "PPD", "keyconfig.ini"), "default keys");
            folder.Write(Path.Combine("data", "PPD", "Lang", "0.ini"), "lang");
            folder.Write(Path.Combine("install", "PPD", "PPD.ini"), "[width]1920\n");
            folder.Write(Path.Combine("install", "PPD", "PPD.exe"), "old exe");

            var executor = Run(new PPDInstallExecutor(folder.Combine("data"), folder.Combine("install"), new SynchronizationContext()));

            Assert.True(executor.Success);
            Assert.Equal("new exe", File.ReadAllText(folder.Combine("install", "PPD", "PPD.exe")));
            Assert.Equal("[width]1920\n", File.ReadAllText(folder.Combine("install", "PPD", "PPD.ini")));
            // 設定ファイルでも、まだないものはコピーする
            Assert.Equal("default keys", File.ReadAllText(folder.Combine("install", "PPD", "keyconfig.ini")));
            Assert.True(File.Exists(folder.Combine("install", "PPD", "Lang", "0.ini")));
        }

        [Fact]
        public void PPD_MissingData_ReportsError()
        {
            using var folder = new TempFolder();

            var executor = Run(new PPDInstallExecutor(folder.Combine("data"), folder.Combine("install"), new SynchronizationContext()));

            Assert.False(executor.Success);
            Assert.StartsWith("PPD Install Error:", executor.ErrorLog);
        }

        [Fact]
        public void BMSTOPPD_CopiesFilesButKeepsExistingSettings()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("data", "BMSTOPPD", "BMSTOPPD.exe"), "exe");
            folder.Write(Path.Combine("data", "BMSTOPPD", "setting.ini"), "default");
            folder.Write(Path.Combine("install", "BMSTOPPD", "setting.ini"), "mine");

            var executor = Run(new BMSTOPPDInstallExecutor(folder.Combine("data"), folder.Path + "\\install", new SynchronizationContext()));

            Assert.True(executor.Success);
            Assert.True(File.Exists(folder.Combine("install", "BMSTOPPD", "BMSTOPPD.exe")));
            Assert.Equal("mine", File.ReadAllText(folder.Combine("install", "BMSTOPPD", "setting.ini")));
        }

        [Fact]
        public void BMSTOPPD_MissingData_ReportsError()
        {
            using var folder = new TempFolder();

            var executor = Run(new BMSTOPPDInstallExecutor(folder.Combine("data"), folder.Combine("install"), new SynchronizationContext()));

            Assert.StartsWith("BMSTOPPD Install Error:", executor.ErrorLog);
        }

        [Fact]
        public void Unzip_ExtractsFoldersAndFiles()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("src", "a.txt"), "a");
            folder.Write(Path.Combine("src", "sub", "b.txt"), "b");
            ZipFile.CreateFromDirectory(folder.Combine("src"), folder.Combine("test.zip"));

            Utility.Unzip(folder.Combine("test.zip"), folder.Combine("out"));

            Assert.Equal("a", File.ReadAllText(folder.Combine("out", "a.txt")));
            Assert.Equal("b", File.ReadAllText(folder.Combine("out", "sub", "b.txt")));
        }

        [Fact]
        public void CopyDirectory_OverwritesExceptProtectedExistingFiles()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("src", "a.ini"), "new");
            folder.Write(Path.Combine("src", "b.txt"), "new");
            folder.Write(Path.Combine("dst", "a.ini"), "old");
            folder.Write(Path.Combine("dst", "b.txt"), "old");

            Utility.CopyDirectory(folder.Combine("src"), folder.Combine("dst") + "\\", [IniFileRegex()]);

            Assert.Equal("old", File.ReadAllText(folder.Combine("dst", "a.ini")));
            Assert.Equal("new", File.ReadAllText(folder.Combine("dst", "b.txt")));
        }

        [GeneratedRegex("\\.ini$")]
        private static partial Regex IniFileRegex();
    }

    /// <summary>
    /// PPDInstaller の ViewModel のテストです。インストール（ダウンロード）は始めないので、確認のページでは次へを押さない。
    /// </summary>
    public sealed class PPDInstallerViewModelTests : IDisposable
    {
        sealed class FakeWindowService : IWindowService
        {
            public string SelectedFolder { get; set; }
            public bool ConfirmAnswer { get; set; }
            public List<string> Confirmed { get; } = [];
            public List<string> Messages { get; } = [];
            public bool Closed { get; private set; }

            public string SelectFolder(string initialDirectory) => SelectedFolder;

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption)
            {
                Confirmed.Add(text);
                return ConfirmAnswer;
            }

            public void Close() => Closed = true;
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeWindowService window = new();
        readonly MainWindowViewModel viewModel;

        public PPDInstallerViewModelTests()
        {
            window.SelectedFolder = folder.Combine("target");
            viewModel = new MainWindowViewModel(window, new LanguageTexts(), folder.Combine("Data"));
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        // カスタムインストールで、インストール先を一時フォルダーにして確認のページの手前まで進める
        private InstallFolderPageViewModel GoToFolderPage()
        {
            ((StartPageViewModel)viewModel.CurrentPage).CustomInstallCommand.Execute(null);
            var folderPage = Assert.IsType<InstallFolderPageViewModel>(viewModel.CurrentPage);
            folderPage.BrowseCommand.Execute(null);
            return folderPage;
        }

        private ConfirmPageViewModel GoToConfirmPage(bool registerStartMenu = false)
        {
            GoToFolderPage();
            viewModel.NextOrCloseCommand.Execute(null);
            Assert.IsType<InstallComponentPageViewModel>(viewModel.CurrentPage);
            viewModel.NextOrCloseCommand.Execute(null);
            var linkPage = Assert.IsType<LinkCreatePageViewModel>(viewModel.CurrentPage);
            linkPage.RegisterStartMenu = registerStartMenu;
            viewModel.NextOrCloseCommand.Execute(null);
            return Assert.IsType<ConfirmPageViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public void StartPage_OnlyCancelIsAvailable()
        {
            var page = Assert.IsType<StartPageViewModel>(viewModel.CurrentPage);

            Assert.False(viewModel.NextOrCloseCommand.CanExecute(null));
            Assert.False(viewModel.PreviousCommand.CanExecute(null));
            Assert.True(viewModel.CancelCommand.CanExecute(null));
            Assert.False(page.IsNextVisible || page.IsPreviousVisible);
            Assert.True(page.IsCancelVisible);
            Assert.Equal("次へ", viewModel.NextText);
        }

        [Fact]
        public void FolderPage_DefaultsToSystemDriveAndBrowseAddsKHC()
        {
            ((StartPageViewModel)viewModel.CurrentPage).CustomInstallCommand.Execute(null);
            var folderPage = Assert.IsType<InstallFolderPageViewModel>(viewModel.CurrentPage);
            Assert.Equal(Environment.SystemDirectory[..2] + "\\KHC", folderPage.InstallFolder);

            window.SelectedFolder = null;
            folderPage.BrowseCommand.Execute(null);
            Assert.Equal(Environment.SystemDirectory[..2] + "\\KHC", folderPage.InstallFolder);

            window.SelectedFolder = folder.Combine("target");
            folderPage.BrowseCommand.Execute(null);
            Assert.Equal(folder.Combine("target") + "\\KHC", folderPage.InstallFolder);
            Assert.Equal(folder.Combine("target", "KHC") + "\\", folderPage.InstallDirectory);
        }

        [Fact]
        public void ConfirmPage_ListsSelectedComponents()
        {
            var confirmPage = GoToConfirmPage();

            Assert.Equal(["必須", "任意"], confirmPage.Nodes.Select(n => n.Text));
            Assert.Contains("PPD", confirmPage.Nodes[0].Children.Select(n => n.Text));
            Assert.Equal(["BMSTOPPD", "LAVFilters"], confirmPage.Nodes[1].Children.Select(n => n.Text));
            Assert.All(confirmPage.Nodes.SelectMany(n => n.Children), n => Assert.Equal(InstallState.NotYet, n.State));
            Assert.Equal("以下をダウンロードしインストールします", confirmPage.StatusText);
        }

        [Fact]
        public void ComponentPage_UnchecksSameVersionAlreadyInstalled()
        {
            var exe = typeof(InstallInfo).Assembly.Location;
            Directory.CreateDirectory(folder.Combine("Data", "PPD"));
            File.Copy(exe, folder.Combine("Data", "PPD", "PPD.exe"));
            var version = FileVersionInfo.GetVersionInfo(exe).FileVersion;
            folder.Write(Path.Combine("target", "KHC", "PPD", "install.info"), $"[PPDVersion]{version}\n[BMSTOPPDVersion]0.0.0.1\n");
            // 版はインストーラーを作ったときに読むので、作り直す
            var installer = new MainWindowViewModel(window, new LanguageTexts(), folder.Combine("Data"));
            ((StartPageViewModel)installer.CurrentPage).CustomInstallCommand.Execute(null);
            ((InstallFolderPageViewModel)installer.CurrentPage).BrowseCommand.Execute(null);

            installer.NextOrCloseCommand.Execute(null);

            var componentPage = Assert.IsType<InstallComponentPageViewModel>(installer.CurrentPage);
            Assert.False(componentPage.InstallPPD);
            Assert.True(componentPage.InstallBMSTOPPD);
            Assert.True(componentPage.InstallLAVFilters);
        }

        [Fact]
        public void ConfirmPage_NothingSelected_ListsOnlyHeadings()
        {
            GoToFolderPage();
            viewModel.NextOrCloseCommand.Execute(null);
            var componentPage = (InstallComponentPageViewModel)viewModel.CurrentPage;
            componentPage.InstallPPD = componentPage.InstallIPAFont = componentPage.InstallBMSTOPPD = componentPage.InstallLAVFilters = false;
            viewModel.NextOrCloseCommand.Execute(null);
            viewModel.NextOrCloseCommand.Execute(null);

            var confirmPage = Assert.IsType<ConfirmPageViewModel>(viewModel.CurrentPage);
            Assert.All(confirmPage.Nodes, n => Assert.Empty(n.Children));
        }

        [Fact]
        public void Previous_GoesBack()
        {
            GoToConfirmPage();

            viewModel.PreviousCommand.Execute(null);

            Assert.IsType<LinkCreatePageViewModel>(viewModel.CurrentPage);
            Assert.True(viewModel.PreviousCommand.CanExecute(null));
        }

        [Fact]
        public void Cancel_ClosesWindow()
        {
            viewModel.CancelCommand.Execute(null);

            Assert.True(window.Closed);
        }

        [Fact]
        public void CanClose_AnsweredNo_StaysOnPage()
        {
            GoToFolderPage();

            Assert.False(viewModel.CanClose());

            Assert.Equal(["インストールを終了してもよろしいですか？"], window.Confirmed);
            Assert.IsType<InstallFolderPageViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public void CanClose_AnsweredYes_ShowsAbortedPageThenCloses()
        {
            window.ConfirmAnswer = true;

            Assert.False(viewModel.CanClose());

            var page = Assert.IsType<InstallAbortedPageViewModel>(viewModel.CurrentPage);
            Assert.Equal("終了", viewModel.NextText);
            Assert.False(viewModel.CancelCommand.CanExecute(null) || viewModel.PreviousCommand.CanExecute(null));
            Assert.False(page.IsCancelVisible || page.IsPreviousVisible);
            viewModel.NextOrCloseCommand.Execute(null);
            Assert.True(window.Closed);
            Assert.True(viewModel.CanClose());
        }

        [Fact]
        public void InstallFinished_WritesInstallInfoAndShowsFinishPage()
        {
            var confirmPage = GoToConfirmPage(registerStartMenu: false);
            var installDirectory = folder.Combine("target", "KHC");
            folder.Write(Path.Combine("target", "KHC", "PPD", "PPD.ini"), "[fontname]Old\n");
            // ダウンロードせずに、全部の項目が終わったところから続ける
            typeof(ConfirmPageViewModel).GetMethod("ExecuteNext", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(confirmPage, null);

            Assert.True(confirmPage.IsInstallFinished);
            var finishPage = Assert.IsType<FinishPageViewModel>(viewModel.CurrentPage);
            Assert.False(finishPage.HasError);
            Assert.False(viewModel.IsInstalling);
            Assert.Equal("終了", viewModel.NextText);
            var info = new SettingReader(File.ReadAllText(Path.Combine(installDirectory, "PPD", "install.info")));
            Assert.Equal(("1", "0"), (info["PPD"], info["RegisterStartMenu"]));
            Assert.Equal("jp", new SettingReader(File.ReadAllText(Path.Combine(installDirectory, "PPD", "PPD.ini")))["Language"]);
            Assert.Empty(window.Messages);
            viewModel.NextOrCloseCommand.Execute(null);
            Assert.True(window.Closed);
        }

        [Fact]
        public void LocalInstall_CopiesAndReportsFailedItems()
        {
            GoToFolderPage();
            viewModel.NextOrCloseCommand.Execute(null);
            var componentPage = (InstallComponentPageViewModel)viewModel.CurrentPage;
            // ダウンロードするもの（IPA フォント、LAVFilters）は選ばない
            componentPage.InstallPPD = componentPage.InstallBMSTOPPD = true;
            componentPage.InstallIPAFont = componentPage.InstallLAVFilters = false;
            viewModel.NextOrCloseCommand.Execute(null);
            ((LinkCreatePageViewModel)viewModel.CurrentPage).RegisterStartMenu = false;
            viewModel.NextOrCloseCommand.Execute(null);
            var confirmPage = Assert.IsType<ConfirmPageViewModel>(viewModel.CurrentPage);
            folder.Write(Path.Combine("Data", "PPD", "PPD.exe"), "exe");
            var statusTexts = new List<string>();
            confirmPage.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ConfirmPageViewModel.StatusText)) statusTexts.Add(confirmPage.StatusText);
            };
            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            try
            {
                // ダウンロードのリストを取ってきたところから続ける（Data に BMSTOPPD がないので、BMSTOPPD は失敗する）
                typeof(ConfirmPageViewModel).GetMethod("InstallListExecutor_Finished", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(confirmPage, [null, EventArgs.Empty]);
                Assert.True(SpinWait.SpinUntil(() => viewModel.CurrentPage is FinishPageViewModel, TimeSpan.FromSeconds(10)));
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }

            Assert.Equal("exe", File.ReadAllText(folder.Combine("target", "KHC", "PPD", "PPD.exe")));
            Assert.Equal([InstallState.OK], confirmPage.Nodes[0].Children.Select(n => n.State));
            Assert.Equal([InstallState.NotOK], confirmPage.Nodes[1].Children.Select(n => n.State));
            Assert.Equal(["インストールしています... PPD 0%", "インストールしています... BMSTOPPD 0%"], statusTexts);
            var finishPage = (FinishPageViewModel)viewModel.CurrentPage;
            Assert.StartsWith("BMSTOPPD Install Error:", finishPage.ErrorText);
            Assert.True(finishPage.HasError);
        }

        [Fact]
        public void FinishPage_ShowsError()
        {
            var page = new FinishPageViewModel(new LanguageTexts());
            var changed = new List<string>();
            page.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

            page.ErrorText = "PPD Install Error";

            Assert.True(page.HasError);
            Assert.Contains(nameof(FinishPageViewModel.HasError), changed);
            Assert.False(page.CanPrevious || page.CanCancel);
        }
    }
}
