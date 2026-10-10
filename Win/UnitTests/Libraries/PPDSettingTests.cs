using PPDFramework;
using System.IO;
using System.Reflection;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ゲームの設定（PPD.ini）を読む PPDSetting のテストです。
    /// </summary>
    /// <remarks>
    /// 全体の設定（PPDSetting.Setting）は変えず、一時フォルダーのテスト用の PPD.ini から別に作る。
    /// </remarks>
    public class PPDSettingTests
    {
        static PPDSetting Create(bool isDebug = false, string songDir = "", string langIso = null, bool disableExpansion = false)
        {
            var constructor = typeof(PPDSetting).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(bool), typeof(string), typeof(string), typeof(bool)]);
            return (PPDSetting)constructor.Invoke([isDebug, songDir, langIso, disableExpansion]);
        }

        const string Ini = "[skinname]TestSkin\n[adjustgaptime]0.25\n[fontname]TestFont\n[fontsize]24\n[width]1280\n[height]720\n[movielatency]0.5\n"
            + "[token]dummy-token\n[tokensecret]dummy-secret\n[songdir]my songs\n[Language]en\n[textboxdisabled]1\n[drawsamecoloratsametimingdisabled]1\n"
            + "[fullscreen]1\n[movieextensions]MP4;avi\n[musicextensions]ogg\n[allowedtousemuchmemory]1\n[IsDebug]1\n[drawconnectunderallmark]1\n"
            + "[everyframepollingdisabled]1\n[menumoviepreviewdisabled]1\n[accurateinputsleeptime]3\n[collectlog]1\n[runexpansion]1\n"
            + "[expansionwaitport]12345\n[autoadjustlatencydisabled]1\n[fontscaledisabled]1\n[highresolutionimagedisabled]1\n[texturecharmode]1\n[fixedfpsdisabled]1\n";

        [Fact]
        public void ReadsEverySetting()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write("PPD.ini", Ini);

            var setting = Create();

            Assert.Equal(("TestSkin", 0.25f, "TestFont", 24, 1280, 720, 0.5f), (setting.SkinName, setting.AdjustGapTime, setting.FontName, setting.FontSizeRatio, setting.Width, setting.Height, setting.MovieLatency));
            Assert.Equal(("my songs", "en", true, true, true), (setting.SongDir, setting.LangISO, setting.TextBoxDisabled, setting.DrawSameColorAtSameTimingDisabled, setting.FullScreen));
            Assert.Equal(["mp4", "avi"], setting.MovieExtensions);
            Assert.Equal(["ogg"], setting.MusicExtensions);
            Assert.True(setting.AllowedToUseMuchMemory && setting.IsDebug && setting.DrawConnectUnderAllMark && setting.EveryFramePollingDisabled
                && setting.MenuMoviePreviewDisabled && setting.CollectLog && setting.RunExpansion && setting.AutoAdjustLatencyDisabled
                && setting.FontScaleDisabled && setting.HighResolutionImageDisabled && setting.FixedFPSDisabled);
            Assert.Equal((3, 12345, TextureCharMode.WinAPI), (setting.AccurateInputSleepTime, setting.ExpansionWaitPort, setting.TextureCharMode));
            // 判定の幅は PPD.ini では変えない
            Assert.Equal((0.04f, 0.08f, 0.12f, 0.15f), (setting.CoolArea, setting.GoodArea, setting.SafeArea, setting.SadArea));

            // 動画かどうかは拡張子で（大文字小文字は区別しない）
            Assert.True(setting.IsMovie(Path.Combine("a", "movie.MP4")));
            Assert.False(setting.IsMovie("music.ogg"));
            Assert.False(setting.IsMovie("noextension"));
            // 文字の大きさは全体の設定の倍率で決める
            Assert.Equal(PPDSetting.Setting.FontSizeRatio * 64 / 32f, setting.GetAdjustedFontSize(64));

            // エラーの報告に入れる設定の一覧に、ログインの情報は入れない
            var raw = setting.CurrentRaw;
            Assert.Contains("[SkinName]TestSkin", raw);
            Assert.DoesNotContain("dummy-token", raw);
            Assert.DoesNotContain("dummy-secret", raw);
        }

        [Fact]
        public void Options_OverrideTheFile()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write("PPD.ini", Ini);
            var songs = folder.Combine("debug songs");
            Directory.CreateDirectory(songs);

            // デバッグでは譜面のフォルダーを変えられる。Expansion を止め、言語を変える
            var setting = Create(true, songs, "cn", true);

            Assert.Equal((true, songs, "cn", false), (setting.IsDebug, setting.SongDir, setting.LangISO, setting.RunExpansion));
            // ないフォルダーは使わない
            Assert.Equal("my songs", Create(true, folder.Combine("missing")).SongDir);
        }

        [Fact]
        public void WithoutFile_UsesDefaults()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write("PPD.ini", "[songdir]\n");

            var empty = Create();
            Assert.Equal("songs", empty.SongDir);
            File.Delete("PPD.ini");

            var setting = Create();

            Assert.Equal(("songs", "IPAGothic", "jp", 30, 32), (setting.SongDir, setting.FontName, setting.LangISO, setting.FontSizeRatio, setting.BaseSize));
            Assert.Empty(setting.MovieExtensions);
            Assert.False(setting.IsMovie("a.mp4"));
            Assert.Equal(4, PPDSetting.DefaultSounds.Length);
        }
    }
}
