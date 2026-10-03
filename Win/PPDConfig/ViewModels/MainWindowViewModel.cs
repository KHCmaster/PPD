using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PPDConfig.ViewModels
{
    /// <summary>
    /// 画面に頼む操作です。ウィンドウが実装します。
    /// </summary>
    public interface IWindowService
    {
        void ShowMessage(string text);

        void Close();
    }

    /// <summary>
    /// PPD の設定（PPD.ini）を編集します。入力欄の値は入力したままの文字列で持ち、保存するときに数値にします。
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        const int DefaultExpansionPort = 54320;
        const int MaxAccurateInputSleepTime = 16;

        readonly IWindowService windowService;
        readonly string ppdIniPath;
        readonly string settingPath;
        string langFileISO = "jp";

        public LanguageTexts Texts { get; } = new();

        public ObservableCollection<LanguageOption> Languages { get; } = [];

        /// <summary>
        /// 高精度入力時のポーリングの間隔の選択肢（ms、0 は無制限）。
        /// </summary>
        public IReadOnlyList<int> AccurateInputSleepTimes { get; } = [.. Enumerable.Range(0, MaxAccurateInputSleepTime + 1)];

        [ObservableProperty] private string windowSize = "800,450";
        [ObservableProperty] private bool fullScreen;
        [ObservableProperty] private bool drawConnectUnderAllMark;
        [ObservableProperty] private string monitorLatency = "0";
        [ObservableProperty] private string movieLatency = "0";
        [ObservableProperty] private string movieExtensions = "";
        [ObservableProperty] private string musicExtensions = "";
        [ObservableProperty] private string fontName = "";
        [ObservableProperty] private string fontSize = "30";
        [ObservableProperty] private string blueSkyId = "";
        [ObservableProperty] private string blueSkyPassword = "";
        [ObservableProperty] private bool textBoxDisabled;
        [ObservableProperty] private bool drawSameColorDisabled;
        [ObservableProperty] private bool allowedToUseMuchMemory;
        [ObservableProperty] private bool everyFramePollingDisabled;
        [ObservableProperty] private int accurateInputSleepTime;
        [ObservableProperty] private bool menuMoviePreviewDisabled;
        [ObservableProperty] private bool autoAdjustLatencyDisabled;
        [ObservableProperty] private bool fontScaleDisabled;
        [ObservableProperty] private bool highResolutionImageDisabled;
        [ObservableProperty] private bool fixedFPSDisabled;
        [ObservableProperty] private bool runExpansion;
        [ObservableProperty] private string expansionPort = DefaultExpansionPort.ToString();

        /// <param name="ppdIniPath">PPD の設定のファイル（PPD.ini）。</param>
        /// <param name="settingPath">このアプリの設定のファイル（言語を保存する）。</param>
        public MainWindowViewModel(IWindowService windowService, string ppdIniPath, string settingPath)
        {
            this.windowService = windowService;
            this.ppdIniPath = ppdIniPath;
            this.settingPath = settingPath;
            if (File.Exists(settingPath))
            {
                langFileISO = new SettingReader(File.ReadAllText(settingPath)).ReadString("Language");
            }
            LoadLanguages();
            ChangeLanguage(langFileISO);
            Load();
        }

        private void LoadLanguages()
        {
            const string componentName = "PPDConfig";
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
            {
                var m = LangFileRegex().Match(Path.GetFileName(fileName));
                if (!m.Success || !Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                {
                    continue;
                }
                var displayName = new SettingReader(File.ReadAllText(fileName)).ReadString("DisplayName");
                Languages.Add(new LanguageOption(displayName, m.Groups["ISO"].Value));
            }
        }

        [RelayCommand]
        private void SelectLanguage(LanguageOption option)
        {
            ChangeLanguage(option.Iso);
        }

        private void ChangeLanguage(string iso)
        {
            langFileISO = iso;
            Utility.ChangeLanguage(iso);
            foreach (var language in Languages)
            {
                language.IsChecked = language.Iso.Equals(iso, StringComparison.CurrentCultureIgnoreCase);
            }
            Texts.Refresh();
        }

        private void Load()
        {
            if (!File.Exists(ppdIniPath))
            {
                return;
            }
            try
            {
                var setting = new SettingReader(File.ReadAllText(ppdIniPath));
                MonitorLatency = setting.ReadFloat("adjustgaptime", 0).ToString();
                FontName = setting.ReadString("fontname");
                FontSize = setting.ReadInt("fontsize", 30).ToString();
                WindowSize = String.Format("{0},{1}", setting.ReadInt("width", 800), setting.ReadInt("height", 450));
                MovieLatency = setting.ReadFloat("movielatency", 0).ToString();
                MovieExtensions = setting.ReadString("movieextensions");
                MusicExtensions = setting.ReadString("musicextensions");
                TextBoxDisabled = setting.ReadString("textboxdisabled") == "1";
                DrawSameColorDisabled = setting.ReadString("drawsamecoloratsametimingdisabled") == "1";
                FullScreen = setting.ReadString("fullscreen") == "1";
                AllowedToUseMuchMemory = setting.ReadString("allowedtousemuchmemory") == "1";
                DrawConnectUnderAllMark = setting.ReadString("drawconnectunderallmark") == "1";
                EveryFramePollingDisabled = setting.ReadString("everyframepollingdisabled") == "1";
                MenuMoviePreviewDisabled = setting.ReadString("menumoviepreviewdisabled") == "1";
                AccurateInputSleepTime = Math.Clamp(setting.ReadInt("accurateinputsleeptime", 0), 0, MaxAccurateInputSleepTime);
                RunExpansion = setting.ReadString("runexpansion") == "1";
                ExpansionPort = setting.ReadInt("expansionwaitport", DefaultExpansionPort).ToString();
                AutoAdjustLatencyDisabled = setting.ReadBoolean("autoadjustlatencydisabled");
                FontScaleDisabled = setting.ReadBoolean("fontscaledisabled");
                HighResolutionImageDisabled = setting.ReadBoolean("highresolutionimagedisabled");
                FixedFPSDisabled = setting.ReadBoolean("fixedfpsdisabled");
                BlueSkyId = setting.ReadString("blueskyid");
                BlueSkyPassword = setting.ReadString("blueskypassword");
            }
            catch (Exception e)
            {
                windowService.ShowMessage(e.Message);
            }
        }

        private void Save()
        {
            var setting = new SettingReader(File.Exists(ppdIniPath) ? File.ReadAllText(ppdIniPath) : "");
            setting.ReplaceOrAdd("d3d", 0);
            setting.ReplaceOrAdd("adjustgaptime", ParseFloat(MonitorLatency));
            setting.ReplaceOrAdd("fontname", FontName);
            setting.ReplaceOrAdd("fontsize", ParseInt(FontSize));
            var size = WindowSize.Split(',');
            if (size.Length != 2 || !int.TryParse(size[0], out int width) || !int.TryParse(size[1], out int height))
            {
                width = 800;
                height = 450;
            }
            setting.ReplaceOrAdd("width", width);
            setting.ReplaceOrAdd("height", height);
            setting.ReplaceOrAdd("movielatency", ParseFloat(MovieLatency));
            setting.ReplaceOrAdd("movieextensions", MovieExtensions.Trim());
            setting.ReplaceOrAdd("musicextensions", MusicExtensions.Trim());
            setting.ReplaceOrAdd("Language", langFileISO);
            setting.ReplaceOrAdd("textboxdisabled", TextBoxDisabled);
            setting.ReplaceOrAdd("drawsamecoloratsametimingdisabled", DrawSameColorDisabled);
            setting.ReplaceOrAdd("fullscreen", FullScreen);
            setting.ReplaceOrAdd("allowedtousemuchmemory", AllowedToUseMuchMemory);
            setting.ReplaceOrAdd("drawconnectunderallmark", DrawConnectUnderAllMark);
            setting.ReplaceOrAdd("everyframepollingdisabled", EveryFramePollingDisabled);
            setting.ReplaceOrAdd("menumoviepreviewdisabled", MenuMoviePreviewDisabled);
            setting.ReplaceOrAdd("accurateinputsleeptime", AccurateInputSleepTime);
            setting.ReplaceOrAdd("runexpansion", RunExpansion);
            setting.ReplaceOrAdd("expansionwaitport", ParseInt(ExpansionPort));
            setting.ReplaceOrAdd("autoadjustlatencydisabled", AutoAdjustLatencyDisabled);
            setting.ReplaceOrAdd("fontscaledisabled", FontScaleDisabled);
            setting.ReplaceOrAdd("highresolutionimagedisabled", HighResolutionImageDisabled);
            setting.ReplaceOrAdd("fixedfpsdisabled", FixedFPSDisabled);
            setting.ReplaceOrAdd("blueskyid", BlueSkyId);
            setting.ReplaceOrAdd("blueskypassword", BlueSkyPassword);
            Write(ppdIniPath, setting);

            var appSetting = new SettingReader(File.Exists(settingPath) ? File.ReadAllText(settingPath) : "");
            appSetting.ReplaceOrAdd("Language", langFileISO);
            Write(settingPath, appSetting);
        }

        private static void Write(string path, SettingReader setting)
        {
            using var sw = new SettingWriter(path, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }

        private static float ParseFloat(string text)
        {
            _ = float.TryParse(text, out float value);
            return value;
        }

        private static int ParseInt(string text)
        {
            _ = int.TryParse(text, out int value);
            return value;
        }

        [RelayCommand]
        private void Ok()
        {
            Save();
            windowService.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            windowService.Close();
        }

        [RelayCommand]
        private void SetDefault()
        {
            MonitorLatency = 0.017f.ToString();
            MovieExtensions = "mp4;flv;avi;wmv;mkv;divx;mov;ogv;webm";
            MusicExtensions = "wav;mp3;ogg;flac;aac";
            FontName = "IPAGothic";
            FontSize = "30";
            WindowSize = "800,450";
            MovieLatency = "0";
            TextBoxDisabled = false;
            DrawSameColorDisabled = false;
            FullScreen = false;
            AllowedToUseMuchMemory = false;
            DrawConnectUnderAllMark = false;
            EveryFramePollingDisabled = false;
            MenuMoviePreviewDisabled = false;
            AccurateInputSleepTime = 0;
            RunExpansion = false;
            ExpansionPort = DefaultExpansionPort.ToString();
            AutoAdjustLatencyDisabled = false;
            FontScaleDisabled = false;
            HighResolutionImageDisabled = false;
            FixedFPSDisabled = false;
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
    }
}
