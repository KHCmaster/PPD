using PPDConfig.ViewModels;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDConfig
{
    /// <summary>
    /// PPDConfig の ViewModel のテストです。PPD.ini は一時フォルダにテスト用のものを作って使う（本物は使わない）。
    /// </summary>
    public class PPDConfigViewModelTests : IDisposable
    {
        sealed class FakeWindowService : IWindowService
        {
            public List<string> Messages { get; } = [];
            public bool Closed { get; private set; }

            public void ShowMessage(string text) => Messages.Add(text);

            public void Close() => Closed = true;
        }

        const string dummyIni = "[width]1280\n[height]720\n[adjustgaptime]0.5\n[fontname]TestFont\n[fontsize]24\n[movielatency]0.25\n"
            + "[movieextensions]mp4\n[musicextensions]ogg\n[fullscreen]1\n[textboxdisabled]1\n[drawsamecoloratsametimingdisabled]1\n"
            + "[allowedtousemuchmemory]1\n[drawconnectunderallmark]1\n[everyframepollingdisabled]1\n[menumoviepreviewdisabled]1\n"
            + "[accurateinputsleeptime]99\n[runexpansion]1\n[expansionwaitport]12345\n[autoadjustlatencydisabled]1\n"
            + "[fontscaledisabled]1\n[highresolutionimagedisabled]1\n[fixedfpsdisabled]1\n[blueskyid]dummy-id\n[blueskypassword]dummy-password\n"
            + "[othersetting]keep\n";

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeWindowService window = new();

        public PPDConfigViewModelTests()
        {
            AppFiles.CopyTo("PPDConfig", folder);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private MainWindowViewModel Create()
        {
            return new MainWindowViewModel(window, "PPD.ini", "PPDConfig.ini");
        }

        private static SettingReader ReadIni(string path)
        {
            return new SettingReader(File.ReadAllText(path));
        }

        [Fact]
        public void Startup_WithoutIni_UsesDefaults()
        {
            var viewModel = Create();

            Assert.Equal("800,450", viewModel.WindowSize);
            Assert.Equal("30", viewModel.FontSize);
            Assert.False(viewModel.FullScreen);
            Assert.Equal("54320", viewModel.ExpansionPort);
            Assert.Equal(17, viewModel.AccurateInputSleepTimes.Count);
            Assert.Empty(window.Messages);
        }

        [Fact]
        public void Startup_LoadsIni()
        {
            File.WriteAllText("PPD.ini", dummyIni);

            var viewModel = Create();

            Assert.Equal("1280,720", viewModel.WindowSize);
            Assert.Equal("0.5", viewModel.MonitorLatency);
            Assert.Equal("0.25", viewModel.MovieLatency);
            Assert.Equal(("mp4", "ogg"), (viewModel.MovieExtensions, viewModel.MusicExtensions));
            Assert.Equal(("TestFont", "24"), (viewModel.FontName, viewModel.FontSize));
            Assert.True(viewModel.FullScreen && viewModel.TextBoxDisabled && viewModel.DrawSameColorDisabled && viewModel.AllowedToUseMuchMemory);
            Assert.True(viewModel.DrawConnectUnderAllMark && viewModel.EveryFramePollingDisabled && viewModel.MenuMoviePreviewDisabled);
            Assert.True(viewModel.AutoAdjustLatencyDisabled && viewModel.FontScaleDisabled && viewModel.HighResolutionImageDisabled && viewModel.FixedFPSDisabled);
            Assert.True(viewModel.RunExpansion);
            Assert.Equal("12345", viewModel.ExpansionPort);
            // 範囲の外の値は、範囲に収める
            Assert.Equal(16, viewModel.AccurateInputSleepTime);
            Assert.Equal(("dummy-id", "dummy-password"), (viewModel.BlueSkyId, viewModel.BlueSkyPassword));
        }

        [Fact]
        public void Ok_SavesValuesKeepsOtherSettingsAndCloses()
        {
            File.WriteAllText("PPD.ini", dummyIni);
            var viewModel = Create();
            viewModel.WindowSize = "1920,1080";
            viewModel.FullScreen = false;
            viewModel.FontSize = "40";
            viewModel.AccurateInputSleepTime = 5;
            viewModel.ExpansionPort = "11111";

            viewModel.OkCommand.Execute(null);

            Assert.True(window.Closed);
            var ini = ReadIni("PPD.ini");
            Assert.Equal(("1920", "1080"), (ini["width"], ini["height"]));
            Assert.Equal("0", ini["fullscreen"]);
            Assert.Equal("40", ini["fontsize"]);
            Assert.Equal("5", ini["accurateinputsleeptime"]);
            Assert.Equal("11111", ini["expansionwaitport"]);
            Assert.Equal("0", ini["d3d"]);
            Assert.Equal("keep", ini["othersetting"]);
            Assert.Equal("dummy-id", ini["blueskyid"]);
        }

        [Fact]
        public void Ok_InvalidNumbers_SaveDefaults()
        {
            var viewModel = Create();
            viewModel.WindowSize = "big";
            viewModel.FontSize = "x";
            viewModel.MonitorLatency = "?";

            viewModel.OkCommand.Execute(null);

            var ini = ReadIni("PPD.ini");
            Assert.Equal(("800", "450"), (ini["width"], ini["height"]));
            Assert.Equal("0", ini["fontsize"]);
            Assert.Equal("0", ini["adjustgaptime"]);
        }

        [Fact]
        public void Ok_SavesLanguageToBothFiles()
        {
            var viewModel = Create();
            viewModel.SelectLanguageCommand.Execute(viewModel.Languages.Single(l => l.Iso == "en"));

            viewModel.OkCommand.Execute(null);

            Assert.Equal("en", ReadIni("PPD.ini")["Language"]);
            Assert.Equal("en", ReadIni("PPDConfig.ini")["Language"]);
        }

        [Fact]
        public void Startup_UsesSavedLanguage()
        {
            File.WriteAllText("PPDConfig.ini", "[Language]en\n");

            var viewModel = Create();

            Assert.True(viewModel.Languages.Single(l => l.Iso == "en").IsChecked);
            Assert.Equal("Language", viewModel.Texts["Language"]);
        }

        [Fact]
        public void SelectLanguage_ChecksOnlyOne()
        {
            var viewModel = Create();

            viewModel.SelectLanguageCommand.Execute(viewModel.Languages.Single(l => l.Iso == "en"));

            Assert.Single(viewModel.Languages, l => l.IsChecked);
            Assert.Equal("Language", viewModel.Texts["Language"]);
        }

        [Fact]
        public void Cancel_ClosesWithoutSaving()
        {
            var viewModel = Create();
            viewModel.FontSize = "99";

            viewModel.CancelCommand.Execute(null);

            Assert.True(window.Closed);
            Assert.False(File.Exists("PPD.ini"));
        }

        [Fact]
        public void SetDefault_ResetsValuesButNotAccount()
        {
            File.WriteAllText("PPD.ini", dummyIni);
            var viewModel = Create();

            viewModel.SetDefaultCommand.Execute(null);

            Assert.Equal("800,450", viewModel.WindowSize);
            Assert.Equal("IPAGothic", viewModel.FontName);
            Assert.Equal(0.017f.ToString(), viewModel.MonitorLatency);
            Assert.False(viewModel.FullScreen || viewModel.TextBoxDisabled || viewModel.RunExpansion || viewModel.FixedFPSDisabled);
            Assert.Equal(0, viewModel.AccurateInputSleepTime);
            Assert.Equal("54320", viewModel.ExpansionPort);
            // アカウントの情報は戻さない
            Assert.Equal("dummy-id", viewModel.BlueSkyId);
            Assert.Equal(dummyIni, File.ReadAllText("PPD.ini"));
        }
    }
}
