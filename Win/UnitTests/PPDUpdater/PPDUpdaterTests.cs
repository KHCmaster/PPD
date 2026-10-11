using PPDUpdater;
using PPDUpdater.Model;
using PPDUpdater.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDUpdater
{
    public class VersionInfoTests
    {
        [Theory]
        [InlineData("1.2.3.4", 1, 2, 3, 4)]
        [InlineData("2.10", 2, 10, 0, 0)]
        [InlineData("1234", 1, 2, 3, 4)]
        [InlineData("12", 1, 2, 0, 0)]
        [InlineData("", 0, 0, 0, 0)]
        [InlineData("a.b.3", 0, 0, 3, 0)]
        public void Parse(string text, int major, int minor, int build, int revision)
        {
            var version = new VersionInfo(text);

            Assert.Equal((major, minor, build, revision), (version.Major, version.Minor, version.Build, version.Revision));
            Assert.Equal($"{major}.{minor}.{build}.{revision}", version.ToString());
        }

        [Theory]
        [InlineData("1.0.0.0", "2.0.0.0", -1)]
        [InlineData("1.2.0.0", "1.1.0.0", 1)]
        [InlineData("1.1.3.0", "1.1.4.0", -1)]
        [InlineData("1.1.1.9", "1.1.1.2", 1)]
        [InlineData("3.2.1.0", "3.2.1.0", 0)]
        public void Compare(string a, string b, int sign)
        {
            var x = new VersionInfo(a);
            var y = new VersionInfo(b);

            Assert.Equal(sign, Math.Sign(x.CompareTo(y)));
            Assert.Equal(sign, Math.Sign(VersionInfoComparer.Comparer.Compare(x, y)));
            Assert.Equal(sign, Math.Sign(UpdateInfoComparer.Comparer.Compare(new UpdateInfo(a, ""), new UpdateInfo(b, ""))));
        }

        [Fact]
        public void Compare_WithNull()
        {
            Assert.Equal(1, VersionInfo.Zero.CompareTo(null));
            Assert.Equal(0, VersionInfoComparer.Comparer.Compare(null, null));
            Assert.Equal(-1, VersionInfoComparer.Comparer.Compare(null, VersionInfo.Zero));
            Assert.Equal(1, VersionInfoComparer.Comparer.Compare(VersionInfo.Zero, null));
        }

        [Fact]
        public void UpdateInfo_ShowsVersion()
        {
            var info = new UpdateInfo("1.2.3.4", "http://example.invalid/a.zip");

            Assert.Equal("1.2.3.4", info.ToString());
            Assert.Equal(AssemblyType.x64, info.AssemblyType);
        }
    }

    public class InstallInfoFileTests
    {
        [Fact]
        public void Load_MissingFile_IsEmpty()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var info = InstallInfoFile.Load();

            Assert.False(info.PPD);
        }

        [Fact]
        public void SaveThenLoad_KeepsEverything()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var info = new InstallInfo
            {
                PPD = true,
                BMSTOPPD = true,
                Effect2DEditor = true,
                IPAFont = true,
                PPDVersion = new VersionInfo("1.2.3.4"),
                BMSTOPPDVersion = new VersionInfo("5.6.7.8"),
                PPDeditorVersion = new VersionInfo("1.0"),
                Effect2DEditorVersion = new VersionInfo("2.0"),
                InstallVersion = new VersionInfo("9.9.9.9"),
            };

            InstallInfoFile.Save(info);
            var loaded = InstallInfoFile.Load();

            Assert.True(loaded.PPD);
            // 以前は保存していなかったので、更新すると BMSTOPPD が更新の対象から外れていた
            Assert.True(loaded.BMSTOPPD);
            Assert.True(loaded.Effect2DEditor);
            Assert.True(loaded.IPAFont);
            Assert.False(loaded.PPDeditor);
            Assert.Equal("5.6.7.8", loaded.BMSTOPPDVersion.ToString());
            Assert.Equal("9.9.9.9", loaded.InstallVersion.ToString());
        }
    }

    public class UpdaterSettingTests
    {
        [Fact]
        public void Load_MissingFile_ReturnsNothing()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var (urls, channel, language) = UpdaterSetting.Load();

            Assert.Empty(urls);
            Assert.Null(channel);
            Assert.Equal("", language);
        }

        [Fact]
        public void Load_ReadsUrlsChannelAndLanguage()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            File.WriteAllText("PPDUpdater.ini", "[UrlList]http://a.invalid/{0}\n[Channel]beta\n[Language]en\n");

            var (urls, channel, language) = UpdaterSetting.Load();

            Assert.Equal(["http://a.invalid/{0}"], urls);
            Assert.Equal("beta", channel);
            Assert.Equal("en", language);
        }

        [Fact]
        public void SaveLanguage_KeepsOtherSettings()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            File.WriteAllText("PPDUpdater.ini", "[Channel]beta\n");

            UpdaterSetting.SaveLanguage("jp");

            var (_, channel, language) = UpdaterSetting.Load();
            Assert.Equal(("beta", "jp"), (channel, language));
        }

        [Fact]
        public void AppendLog_AddsAndStartsOverWhenLarge()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            UpdaterSetting.AppendLog("first");
            UpdaterSetting.AppendLog("second");
            Assert.Contains("first", File.ReadAllText("update.log"));
            Assert.Contains("second", File.ReadAllText("update.log"));

            File.WriteAllText("update.log", new string('x', 1_000_000));
            UpdaterSetting.AppendLog("third");
            Assert.True(new FileInfo("update.log").Length < 1000);
        }
    }

    /// <summary>
    /// PPDUpdater の ViewModel のテストです。更新のリストの場所を設定しないので、ネットワークにはつながない。
    /// </summary>
    public sealed class PPDUpdaterViewModelTests : IDisposable
    {
        sealed class FakeWindowService : IWindowService
        {
            public List<string> Messages { get; } = [];
            public bool ConfirmAnswer { get; set; }
            public bool Closed { get; private set; }

            public void ShowMessage(string message) => Messages.Add(message);

            public bool Confirm(string message, string caption) => ConfirmAnswer;

            public void Close() => Closed = true;
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeWindowService window = new();

        public PPDUpdaterViewModelTests()
        {
            AppFiles.CopyTo("PPDUpdater", folder);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        [Fact]
        public void Startup_ReadsInstalledVersionAndCreatesDownloadFolder()
        {
            File.WriteAllText("install.info", "[InstallVersion]1.2.3.4\n");

            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);

            Assert.Equal("1.2.3.4", viewModel.CurrentVersion);
            Assert.True(Directory.Exists("Update"));
            Assert.True(viewModel.IsInteractive);
            Assert.Equal(6, viewModel.Languages.Count);
            Assert.DoesNotContain(viewModel.Languages, l => l.IsChecked);
        }

        [Fact]
        public void Start_WithoutUpdateList_ShowsErrorAndLog()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);

            viewModel.Start();

            Assert.True(viewModel.IsChecking);
            Assert.True(viewModel.IsLogVisible);
            Assert.Equal(["アップデートリストの取得に失敗しました"], window.Messages);
            Assert.Contains("Error: アップデートリストの取得に失敗しました", viewModel.LogText);
        }

        [Fact]
        public void Start_DownloadOnly_DoesNotShowMessages()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownLoad);

            viewModel.Start();

            Assert.False(viewModel.IsInteractive);
            Assert.Empty(window.Messages);
        }

        [Fact]
        public void Start_InstallOnly_WithNothingDownloaded_Closes()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.Install);

            viewModel.Start();

            Assert.True(window.Closed);
            Assert.Empty(viewModel.Rows);
        }

        [Fact]
        public void SelectLanguage_ChangesTextsAndCheck()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);
            var english = viewModel.Languages.Single(l => l.Iso == "en");

            viewModel.SelectLanguageCommand.Execute(english);

            Assert.True(english.IsChecked);
            Assert.Equal("Language", viewModel.Texts["Language"]);
        }

        [Fact]
        public void Startup_WithSavedLanguage_LoadsIt()
        {
            File.WriteAllText("PPDUpdater.ini", "[Language]en\n");

            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);

            Assert.Equal("Language", viewModel.Texts["Language"]);
            Assert.True(viewModel.Languages.Single(l => l.Iso == "en").IsChecked);
        }

        [Fact]
        public void ToggleLog_ShowsAndHides()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);

            viewModel.ToggleLogCommand.Execute(null);
            Assert.True(viewModel.IsLogVisible);
            viewModel.ToggleLogCommand.Execute(null);
            Assert.False(viewModel.IsLogVisible);
        }

        [Fact]
        public void Close_NothingPending_Closes()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);

            viewModel.CloseCommand.Execute(null);

            Assert.True(window.Closed);
        }

        [Fact]
        public void SaveOnClosing_WritesLogAndLanguage()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);
            viewModel.SelectLanguageCommand.Execute(viewModel.Languages.Single(l => l.Iso == "en"));
            viewModel.Start();

            viewModel.SaveOnClosing();

            Assert.Contains("Error:", File.ReadAllText("update.log"));
            Assert.Contains("[Language]en", File.ReadAllText("PPDUpdater.ini"));
        }

        [Fact]
        public void UpdateRow_StateTextFollowsStateAndLanguage()
        {
            var viewModel = new MainWindowViewModel(window, WorkState.DownloadAndInstall);
            var row = new UpdateRow(new UpdateInfo("1.0.0.0", ""), UpdateState.Waiting, viewModel.Texts);
            var changed = new List<string>();
            row.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

            row.State = UpdateState.InstallFinished;
            viewModel.SelectLanguageCommand.Execute(viewModel.Languages.Single(l => l.Iso == "en"));

            Assert.Equal("1.0.0.0", row.Version);
            Assert.Equal(viewModel.Texts["InstallFinished"], row.StateText);
            Assert.True(changed.Count(name => name == nameof(UpdateRow.StateText)) >= 2);
        }
    }
}
