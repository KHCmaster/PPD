using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PPDConfig
{
    /// <summary>
    /// PPD の設定（PPD.ini）を編集するウィンドウです。
    /// </summary>
    public partial class MainWindow : Window
    {
        const string ppdini = "PPD.ini";
        const string iniFileName = "PPDConfig.ini";
        const int defaultExpansionPort = 54320;
        const int maxAccurateInputSleepTime = 16;

        private string langFileISO = "jp";

        private int formWidth;
        private int formHeight;

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i <= maxAccurateInputSleepTime; i++)
            {
                accurateInputSleepTimeComboBox.Items.Add(i);
            }
            accurateInputSleepTimeComboBox.SelectedIndex = 0;
            CheckSetting();
            CheckLangFiles();
            SetLanguage(langFileISO);
            ReadPPDIni();
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var setting = new SettingReader(File.ReadAllText(iniFileName));
                langFileISO = setting.ReadString("Language");
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
                if (!Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                {
                    continue;
                }
                var setting = new SettingReader(File.ReadAllText(fileName));
                var menuItem = new MenuItem
                {
                    // 名前の「_」がアクセスキーとして扱われないように、TextBlock で表示する
                    Header = new TextBlock { Text = setting.ReadString("DisplayName") },
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
            var m = LangFileRegex().Match(Path.GetFileName((string)menuItem.Tag));
            if (m.Success)
            {
                langFileISO = m.Groups["ISO"].Value;
            }
            SetLanguage(langFileISO);
            foreach (var child in languageMenuItem.Items)
            {
                if (child is MenuItem item)
                {
                    item.IsChecked = false;
                }
            }
            menuItem.IsChecked = true;
        }

        private void SetLanguage(string langIso)
        {
            Utility.ChangeLanguage(langIso);
            languageMenuItem.Header = Utility.Language["Language"];
            monitorLatencyLabel.Text = Utility.Language["Label2"];
            musicExtensionsLabel.Text = Utility.Language["MusicExtensions"];
            fontNameLabel.Text = Utility.Language["Label7"];
            fontSizeLabel.Text = Utility.Language["Label9"];
            windowSizeLabel.Text = Utility.Language["Label13"];
            movieLatencyLabel.Text = Utility.Language["Label14"];
            okButton.Content = Utility.Language["Button5"];
            cancelButton.Content = Utility.Language["Button6"];
            defaultButton.Content = Utility.Language["Button7"];
            fontGroupBox.Header = Utility.Language["GroupBox1"];
            blueSkyIdLabel.Text = Utility.Language["Label16"];
            blueSkyPasswordLabel.Text = Utility.Language["Label18"];
            movieExtensionsLabel.Text = Utility.Language["Label19"];
            generalTab.Header = Utility.Language["TabPage1"];
            blueSkyTab.Header = Utility.Language["TabPage2"];
            disableTab.Header = Utility.Language["TabPage3"];
            textBoxDisabledCheckBox.Content = Utility.Language["CheckBox1"];
            drawSameColorDisabledCheckBox.Content = Utility.Language["CheckBox2"];
            fullScreenCheckBox.Content = Utility.Language["CheckBox3"];
            allowedToUseMuchMemoryCheckBox.Content = Utility.Language["CheckBox5"];
            drawConnectUnderAllMarkCheckBox.Content = Utility.Language["CheckBox6"];
            everyFramePollingDisabledCheckBox.Content = Utility.Language["CheckBox7"];
            menuMoviePreviewDisabledCheckBox.Content = Utility.Language["CheckBox8"];
            accurateInputSleepTimeLabel.Text = Utility.Language["AccurateInputSleepTimeDesc"];
            expansionTab.Header = Utility.Language["ExpansionSetting"];
            runExpansionCheckBox.Content = Utility.Language["Use PPDExpansion"];
            expansionPortLabel.Text = Utility.Language["Using Port"];
            autoAdjustLatencyDisabledCheckBox.Content = Utility.Language["DisableAutoAdjustLatency"];
            fontScaleDisabledCheckBox.Content = Utility.Language["DisableFontScale"];
            highResolutionImageDisabledCheckBox.Content = Utility.Language["DisableHighResolutionImage"];
            fixedFPSDisabledCheckBox.Content = Utility.Language["DisableFixedFPS"];
        }

        private void ReadPPDIni()
        {
            if (!File.Exists(ppdini))
            {
                return;
            }
            try
            {
                var setting = new SettingReader(File.ReadAllText(ppdini));
                MonitorLatency = setting.ReadFloat("adjustgaptime", 0);
                FontName = setting.ReadString("fontname");
                GameFontSize = setting.ReadInt("fontsize", 30);
                FormWidth = setting.ReadInt("width", 800);
                FormHeight = setting.ReadInt("height", 450);
                MovieLatency = setting.ReadFloat("movielatency", 0);
                MovieExtensions = setting.ReadString("movieextensions");
                MusicExtensions = setting.ReadString("musicextensions");
                textBoxDisabledCheckBox.IsChecked = setting.ReadString("textboxdisabled") == "1";
                drawSameColorDisabledCheckBox.IsChecked = setting.ReadString("drawsamecoloratsametimingdisabled") == "1";
                fullScreenCheckBox.IsChecked = setting.ReadString("fullscreen") == "1";
                allowedToUseMuchMemoryCheckBox.IsChecked = setting.ReadString("allowedtousemuchmemory") == "1";
                drawConnectUnderAllMarkCheckBox.IsChecked = setting.ReadString("drawconnectunderallmark") == "1";
                everyFramePollingDisabledCheckBox.IsChecked = setting.ReadString("everyframepollingdisabled") == "1";
                menuMoviePreviewDisabledCheckBox.IsChecked = setting.ReadString("menumoviepreviewdisabled") == "1";
                AccurateInputSleepTime = setting.ReadInt("accurateinputsleeptime", 0);
                runExpansionCheckBox.IsChecked = setting.ReadString("runexpansion") == "1";
                ExpansionWaitPort = setting.ReadInt("expansionwaitport", defaultExpansionPort);
                autoAdjustLatencyDisabledCheckBox.IsChecked = setting.ReadBoolean("autoadjustlatencydisabled");
                fontScaleDisabledCheckBox.IsChecked = setting.ReadBoolean("fontscaledisabled");
                highResolutionImageDisabledCheckBox.IsChecked = setting.ReadBoolean("highresolutionimagedisabled");
                fixedFPSDisabledCheckBox.IsChecked = setting.ReadBoolean("fixedfpsdisabled");
                blueSkyIdTextBox.Text = setting.ReadString("blueskyid");
                blueSkyPasswordBox.Password = setting.ReadString("blueskypassword");
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message);
            }
        }

        private void SavePPDIni()
        {
            var setting = new SettingReader(File.Exists(ppdini) ? File.ReadAllText(ppdini) : "");
            setting.ReplaceOrAdd("d3d", 0);
            setting.ReplaceOrAdd("adjustgaptime", MonitorLatency);
            setting.ReplaceOrAdd("fontname", FontName);
            setting.ReplaceOrAdd("fontsize", GameFontSize);
            var size = windowSizeTextBox.Text.Split(',');
            if (size.Length != 2 || !int.TryParse(size[0], out int width) || !int.TryParse(size[1], out int height))
            {
                width = 800;
                height = 450;
            }
            setting.ReplaceOrAdd("width", width);
            setting.ReplaceOrAdd("height", height);
            setting.ReplaceOrAdd("movielatency", MovieLatency);
            setting.ReplaceOrAdd("movieextensions", MovieExtensions);
            setting.ReplaceOrAdd("musicextensions", MusicExtensions);
            setting.ReplaceOrAdd("Language", langFileISO);
            setting.ReplaceOrAdd("textboxdisabled", IsChecked(textBoxDisabledCheckBox));
            setting.ReplaceOrAdd("drawsamecoloratsametimingdisabled", IsChecked(drawSameColorDisabledCheckBox));
            setting.ReplaceOrAdd("fullscreen", IsChecked(fullScreenCheckBox));
            setting.ReplaceOrAdd("allowedtousemuchmemory", IsChecked(allowedToUseMuchMemoryCheckBox));
            setting.ReplaceOrAdd("drawconnectunderallmark", IsChecked(drawConnectUnderAllMarkCheckBox));
            setting.ReplaceOrAdd("everyframepollingdisabled", IsChecked(everyFramePollingDisabledCheckBox));
            setting.ReplaceOrAdd("menumoviepreviewdisabled", IsChecked(menuMoviePreviewDisabledCheckBox));
            setting.ReplaceOrAdd("accurateinputsleeptime", AccurateInputSleepTime);
            setting.ReplaceOrAdd("runexpansion", IsChecked(runExpansionCheckBox));
            setting.ReplaceOrAdd("expansionwaitport", ExpansionWaitPort);
            setting.ReplaceOrAdd("autoadjustlatencydisabled", IsChecked(autoAdjustLatencyDisabledCheckBox));
            setting.ReplaceOrAdd("fontscaledisabled", IsChecked(fontScaleDisabledCheckBox));
            setting.ReplaceOrAdd("highresolutionimagedisabled", IsChecked(highResolutionImageDisabledCheckBox));
            setting.ReplaceOrAdd("fixedfpsdisabled", IsChecked(fixedFPSDisabledCheckBox));
            setting.ReplaceOrAdd("blueskyid", blueSkyIdTextBox.Text);
            setting.ReplaceOrAdd("blueskypassword", blueSkyPasswordBox.Password);
            using var sw = new SettingWriter(ppdini, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }

        private static bool IsChecked(CheckBox checkBox)
        {
            return checkBox.IsChecked == true;
        }

        private void SaveSetting()
        {
            var setting = new SettingReader(File.Exists(iniFileName) ? File.ReadAllText(iniFileName) : "");
            setting.ReplaceOrAdd("Language", langFileISO);
            using SettingWriter sw = new(iniFileName, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SavePPDIni();
            SaveSetting();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            MonitorLatency = 0.017f;
            MovieExtensions = "mp4;flv;avi;wmv;mkv;divx;mov;ogv;webm";
            MusicExtensions = "wav;mp3;ogg;flac;aac";
            FontName = "IPAGothic";
            GameFontSize = 30;
            FormWidth = 800;
            FormHeight = 450;
            MovieLatency = 0;
            textBoxDisabledCheckBox.IsChecked = false;
            drawSameColorDisabledCheckBox.IsChecked = false;
            fullScreenCheckBox.IsChecked = false;
            allowedToUseMuchMemoryCheckBox.IsChecked = false;
            drawConnectUnderAllMarkCheckBox.IsChecked = false;
            everyFramePollingDisabledCheckBox.IsChecked = false;
            menuMoviePreviewDisabledCheckBox.IsChecked = false;
            AccurateInputSleepTime = 0;
            runExpansionCheckBox.IsChecked = false;
            ExpansionWaitPort = defaultExpansionPort;
            autoAdjustLatencyDisabledCheckBox.IsChecked = false;
            fontScaleDisabledCheckBox.IsChecked = false;
            highResolutionImageDisabledCheckBox.IsChecked = false;
            fixedFPSDisabledCheckBox.IsChecked = false;
        }

        public int FormWidth
        {
            get { return formWidth; }
            set
            {
                if (formWidth != value)
                {
                    formWidth = value;
                    UpdateSize();
                }
            }
        }

        public int FormHeight
        {
            get { return formHeight; }
            set
            {
                if (formHeight != value)
                {
                    formHeight = value;
                    UpdateSize();
                }
            }
        }

        private void UpdateSize()
        {
            windowSizeTextBox.Text = String.Format("{0},{1}", formWidth, formHeight);
        }

        private string FontName
        {
            get { return fontNameTextBox.Text; }
            set { fontNameTextBox.Text = value; }
        }

        private int GameFontSize
        {
            get
            {
                _ = int.TryParse(fontSizeTextBox.Text, out int val);
                return val;
            }
            set { fontSizeTextBox.Text = value.ToString(); }
        }

        private float MonitorLatency
        {
            get
            {
                _ = float.TryParse(monitorLatencyTextBox.Text, out float val);
                return val;
            }
            set { monitorLatencyTextBox.Text = value.ToString(); }
        }

        private float MovieLatency
        {
            get
            {
                _ = float.TryParse(movieLatencyTextBox.Text, out float val);
                return val;
            }
            set { movieLatencyTextBox.Text = value.ToString(); }
        }

        public string MovieExtensions
        {
            get { return movieExtensionsTextBox.Text.Trim(); }
            set { movieExtensionsTextBox.Text = value; }
        }

        public string MusicExtensions
        {
            get { return musicExtensionsTextBox.Text.Trim(); }
            set { musicExtensionsTextBox.Text = value; }
        }

        // 高精度入力時のポーリングの間隔（ms、0 は無制限）。範囲は以前の NumericUpDown と同じ 0〜16
        private int AccurateInputSleepTime
        {
            get { return accurateInputSleepTimeComboBox.SelectedIndex < 0 ? 0 : accurateInputSleepTimeComboBox.SelectedIndex; }
            set { accurateInputSleepTimeComboBox.SelectedIndex = Math.Clamp(value, 0, maxAccurateInputSleepTime); }
        }

        public int ExpansionWaitPort
        {
            get
            {
                _ = int.TryParse(expansionPortTextBox.Text, out int val);
                return val;
            }
            set { expansionPortTextBox.Text = value.ToString(); }
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
    }
}
