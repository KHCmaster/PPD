using PPDConfiguration;
using PPDTwitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PPDConfig
{
    public partial class Form1 : Form
    {
        const string ppdini = "PPD.ini";
        const string filterini = "FilterConfig.ini";
        const string langfile = "lang_PPDConfig.ini";
        const string iniFileName = "PPDConfig.ini";
        const int defaultExpansionPort = 54320;
        string userIconFilter = "JPEG,GIF,PNGファイル(*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png|すべてのファイル(*.*)|*.*";
        PPDTwitterManager twitterManager;

        private string langFileISO = "jp";

        private int formWidth;
        private int formHeight;

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            CheckSetting();
            CheckLangFiles();
            SetLanguage(langFileISO);
            ReadPPDIni();
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var sr = new StreamReader(iniFileName);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                langFileISO = setting.ReadString("Language");
            }
        }

        private void CheckLangFiles()
        {
            if (Directory.Exists("Lang"))
            {
                var componentName = this.GetType().Assembly.GetName().Name;
                foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
                {
                    if (!Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                    {
                        continue;
                    }
                    var sr = new StreamReader(fileName);
                    var setting = new SettingReader(sr.ReadToEnd());
                    sr.ReadToEnd();
                    var name = setting.ReadString("DisplayName");
                    var tsmi = new ToolStripMenuItem
                    {
                        Text = name,
                        Name = fileName.ToLower(),
                        Checked = Path.GetFileName(fileName).ToLower() == String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO).ToLower()
                    };
                    tsmi.Click += tsmi_Click;
                    言語ToolStripMenuItem.DropDownItems.Add(tsmi);
                }
            }
        }

        void tsmi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                var langFileName = tsmi.Name;
                var m = Regex.Match(Path.GetFileName(langFileName), "^lang_\\w+_(?<ISO>\\w+).ini$");
                if (m.Success)
                {
                    langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(langFileISO);
                foreach (ToolStripMenuItem child in (tsmi.OwnerItem as ToolStripMenuItem).DropDownItems)
                {
                    child.Checked = false;
                }
                tsmi.Checked = true;
            }
        }


        private void SetLanguage(string langIso)
        {
            Utility.ChangeLanguage(langIso);
            言語ToolStripMenuItem.Text = Utility.Language["Language"];
            this.label1.Text = Utility.Language["Label1"];
            this.label2.Text = Utility.Language["Label2"];
            this.label3.Text = Utility.Language["MusicExtensions"];
            this.label7.Text = Utility.Language["Label7"];
            this.label9.Text = Utility.Language["Label9"];
            this.label13.Text = Utility.Language["Label13"];
            this.label14.Text = Utility.Language["Label14"];
            this.button5.Text = Utility.Language["Button5"];
            this.button6.Text = Utility.Language["Button6"];
            this.button7.Text = Utility.Language["Button7"];
            this.groupBox1.Text = Utility.Language["GroupBox1"];

            int lastSelection = comboBox1.SelectedIndex;
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add(Utility.Language["ComboBox1"]);
            this.comboBox1.Items.Add(Utility.Language["ComboBox2"]);
            this.comboBox1.Items.Add(Utility.Language["ComboBox3"]);
            this.comboBox1.Items.Add(Utility.Language["ComboBox4"]);
            this.comboBox1.SelectedIndex = lastSelection;

            this.label16.Text = Utility.Language["Label16"];
            this.label17.Text = Utility.Language["Label17"];
            this.label18.Text = Utility.Language["Label18"];
            this.label19.Text = Utility.Language["Label19"];
            this.button1.Text = Utility.Language["Button1"];
            this.button2.Text = Utility.Language["Button2"];
            this.tabPage1.Text = Utility.Language["TabPage1"];
            this.tabPage2.Text = Utility.Language["TabPage2"];
            this.tabPage3.Text = Utility.Language["TabPage3"];
            this.checkBox1.Text = Utility.Language["CheckBox1"];
            this.checkBox2.Text = Utility.Language["CheckBox2"];
            this.checkBox3.Text = Utility.Language["CheckBox3"];
            this.checkBox5.Text = Utility.Language["CheckBox5"];
            this.checkBox6.Text = Utility.Language["CheckBox6"];
            this.checkBox7.Text = Utility.Language["CheckBox7"];
            this.checkBox8.Text = Utility.Language["CheckBox8"];
            this.label4.Text = Utility.Language["AccurateInputSleepTimeDesc"];
            this.tabPage4.Text = Utility.Language["ExpansionSetting"];
            this.checkBox9.Text = Utility.Language["Use PPDExpansion"];
            this.label5.Text = Utility.Language["Using Port"];

            userIconFilter = Utility.Language["UserIconFilter"];

            this.checkBox11.Text = Utility.Language["DisableAutoAdjustLatency"];
            this.checkBox4.Text = Utility.Language["DisableShader"];
            this.button4.Text = Utility.Language["Reset"];
            this.checkBox13.Text = Utility.Language["DisableFontScale"];
            this.checkBox14.Text = Utility.Language["DisableHighResolutionImage"];
            this.checkBox15.Text = Utility.Language["DisableFixedFPS"];
        }

        private void ReadPPDIni()
        {
            if (!File.Exists(ppdini)) return;
            try
            {
                var sr = new StreamReader(ppdini);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                MultiSample = setting.ReadInt("multisample", 0);
                MonitorLatency = setting.ReadFloat("adjustgaptime", 0);
                FontName = setting.ReadString("fontname");
                FontSize = setting.ReadInt("fontsize", 30);
                FormWidth = setting.ReadInt("width", 800);
                FormHeight = setting.ReadInt("height", 450);
                MovieLatency = setting.ReadFloat("movielatency", 0);

                this.Token = setting.ReadString("token");
                this.TokenSecret = setting.ReadString("tokensecret");
                this.MovieExtensions = setting.ReadString("movieextensions");
                this.MusicExtensions = setting.ReadString("musicextensions");
                if (this.Token != "" && this.TokenSecret != "")
                {
                    this.TokenAvailable = true;
                }
                TextBoxDisabled = setting.ReadString("textboxdisabled") == "1";
                DrawSameColorAtSameTimingDisabled = setting.ReadString("drawsamecoloratsametimingdisabled") == "1";
                FullScreen = setting.ReadString("fullscreen") == "1";
                AllowedToUseMuchMemory = setting.ReadString("allowedtousemuchmemory") == "1";
                DrawConnectUnderAllMark = setting.ReadString("drawconnectunderallmark") == "1";
                EveryFramePollingDisabled = setting.ReadString("everyframepollingdisabled") == "1";
                MenuMoviePreviewDisabled = setting.ReadString("menumoviepreviewdisabled") == "1";
                AccurateInputSleepTime = setting.ReadInt("accurateinputsleeptime", 0);
                RunExpansion = setting.ReadString("runexpansion") == "1";
                ExpansionWaitPort = setting.ReadInt("expansionwaitport", defaultExpansionPort);
                AutoAdjustLatencyDisabled = setting.ReadBoolean("autoadjustlatencydisabled");
                ShaderDisabled = setting.ReadBoolean("shaderdisabled");
                FontScaleDisabled = setting.ReadBoolean("fontscaledisabled");
                HighResolutionImageDisabled = setting.ReadBoolean("highresolutionimagedisabled");
                FixedFPSDisabled = setting.ReadBoolean("fixedfpsdisabled");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void SavePPDIni()
        {
            if (!File.Exists(ppdini))
            {
                File.Create(ppdini).Close();
            }
            var sr = new StreamReader(ppdini);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            setting.ReplaceOrAdd("multisample", MultiSample);
            setting.ReplaceOrAdd("d3d", 0);
            setting.ReplaceOrAdd("adjustgaptime", MonitorLatency);
            setting.ReplaceOrAdd("fontname", FontName);
            setting.ReplaceOrAdd("fontsize", FontSize);
            var size = textBox9.Text.Split(',');
            if (size.Length == 2 && int.TryParse(size[0], out int width) && int.TryParse(size[1], out int height))
            {
            }
            else
            {
                width = 800;
                height = 450;
            }
            setting.ReplaceOrAdd("width", width);
            setting.ReplaceOrAdd("height", height);
            setting.ReplaceOrAdd("movielatency", MovieLatency);
            setting.ReplaceOrAdd("token", this.Token);
            setting.ReplaceOrAdd("tokensecret", this.TokenSecret);
            setting.ReplaceOrAdd("movieextensions", this.MovieExtensions);
            setting.ReplaceOrAdd("musicextensions", this.MusicExtensions);
            setting.ReplaceOrAdd("Language", langFileISO);
            setting.ReplaceOrAdd("textboxdisabled", TextBoxDisabled);
            setting.ReplaceOrAdd("drawsamecoloratsametimingdisabled", DrawSameColorAtSameTimingDisabled);
            setting.ReplaceOrAdd("fullscreen", FullScreen);
            setting.ReplaceOrAdd("allowedtousemuchmemory", AllowedToUseMuchMemory);
            setting.ReplaceOrAdd("drawconnectunderallmark", DrawConnectUnderAllMark);
            setting.ReplaceOrAdd("everyframepollingdisabled", EveryFramePollingDisabled);
            setting.ReplaceOrAdd("menumoviepreviewdisabled", MenuMoviePreviewDisabled);
            setting.ReplaceOrAdd("accurateinputsleeptime", AccurateInputSleepTime);
            setting.ReplaceOrAdd("runexpansion", RunExpansion);
            setting.ReplaceOrAdd("expansionwaitport", ExpansionWaitPort);
            setting.ReplaceOrAdd("autoadjustlatencydisabled", AutoAdjustLatencyDisabled);
            setting.ReplaceOrAdd("shaderdisabled", ShaderDisabled);
            setting.ReplaceOrAdd("fontscaledisabled", FontScaleDisabled);
            setting.ReplaceOrAdd("highresolutionimagedisabled", HighResolutionImageDisabled);
            setting.ReplaceOrAdd("fixedfpsdisabled", FixedFPSDisabled);
            var sw = new SettingWriter(ppdini, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
            sw.Close();
        }

        private void SaveSetting()
        {
            if (!File.Exists(iniFileName))
            {
                File.Create(iniFileName).Close();
            }
            var sr = new StreamReader(iniFileName);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            setting.ReplaceOrAdd("Language", langFileISO);
            using (SettingWriter sw = new SettingWriter(iniFileName, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            SavePPDIni();
            SaveSetting();
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.comboBox1.SelectedIndex = 0;
            MonitorLatency = 0.017f;
            MovieExtensions = "mp4;flv;avi;wmv;mkv;divx;mov;ogv;webm";
            MusicExtensions = "wav;mp3;ogg;flac;aac";
            FontName = "IPAGothic";
            FontSize = 30;
            FormWidth = 800;
            FormHeight = 450;
            MovieLatency = 0;
            TextBoxDisabled = false;
            DrawSameColorAtSameTimingDisabled = false;
            FullScreen = false;
            AllowedToUseMuchMemory = false;
            DrawConnectUnderAllMark = false;
            EveryFramePollingDisabled = false;
            MenuMoviePreviewDisabled = false;
            AutoAdjustLatencyDisabled = false;
            ShaderDisabled = false;
            AccurateInputSleepTime = 0;
            RunExpansion = false;
            ExpansionWaitPort = defaultExpansionPort;
            AutoAdjustLatencyDisabled = false;
            ShaderDisabled = false;
            FontScaleDisabled = false;
            HighResolutionImageDisabled = false;
            FixedFPSDisabled = false;

            ResetTwitterSetting();
        }

        private void ResetTwitterSetting()
        {
            TokenAvailable = false;
            Token = "";
            TokenSecret = "";
        }

        #region Twitter

        const string ConsumerKey = "";
        const string ConsumerSecret = "";

        private bool tokenavailable;

        private bool TokenAvailable
        {
            get
            {
                return tokenavailable;
            }
            set
            {
                tokenavailable = value;
                if (tokenavailable)
                {
                    this.button1.Enabled = this.button2.Enabled = this.textBox11.Enabled = false;
                    this.pictureBox1.Visible = this.label18.Visible = true;
                }
                else
                {
                    this.button1.Enabled = this.button2.Enabled = this.textBox11.Enabled = true;
                    this.pictureBox1.Visible = this.label18.Visible = false;
                }
            }
        }

        private string Token
        {
            get;
            set;
        }

        private string TokenSecret
        {
            get;
            set;
        }

        #endregion

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
            this.textBox9.Text = String.Format("{0},{1}", formWidth, formHeight);
        }

        private string FontName
        {
            get
            {
                return this.textBox6.Text;
            }
            set
            {
                this.textBox6.Text = value;
            }
        }

        private int FontSize
        {
            get
            {
                int.TryParse(this.textBox8.Text, out int val);
                return val;
            }
            set
            {
                this.textBox8.Text = value.ToString();
            }
        }

        private float MonitorLatency
        {
            get
            {
                float.TryParse(this.textBox1.Text, out float val);
                return val;
            }
            set
            {
                this.textBox1.Text = value.ToString();
            }
        }

        private float MovieLatency
        {
            get
            {
                float.TryParse(this.textBox10.Text, out float val);
                return val;
            }
            set
            {
                this.textBox10.Text = value.ToString();
            }
        }


        private int MultiSample
        {
            get
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        return 0;
                    case 1:
                        return 2;
                    case 2:
                        return 4;
                    case 3:
                        return 8;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0:
                        comboBox1.SelectedIndex = 0;
                        break;
                    case 2:
                        comboBox1.SelectedIndex = 1;
                        break;
                    case 4:
                        comboBox1.SelectedIndex = 2;
                        break;
                    case 8:
                        comboBox1.SelectedIndex = 3;
                        break;
                }
            }
        }

        public string MovieExtensions
        {
            get
            {
                return textBox2.Text.Trim();
            }
            set
            {
                this.textBox2.Text = value;
            }
        }

        public string MusicExtensions
        {
            get
            {
                return this.textBox3.Text.Trim();
            }
            set
            {
                this.textBox3.Text = value;
            }
        }

        public bool TextBoxDisabled
        {
            get
            {
                return checkBox1.Checked;
            }
            set
            {
                checkBox1.Checked = value;
            }
        }

        public bool DrawSameColorAtSameTimingDisabled
        {
            get
            {
                return checkBox2.Checked;
            }
            set
            {
                checkBox2.Checked = value;
            }
        }

        public bool FullScreen
        {
            get
            {
                return checkBox3.Checked;
            }
            set
            {
                checkBox3.Checked = value;
            }
        }

        public bool AllowedToUseMuchMemory
        {
            get
            {
                return checkBox5.Checked;
            }
            set
            {
                checkBox5.Checked = value;
            }
        }

        public bool DrawConnectUnderAllMark
        {
            get
            {
                return checkBox6.Checked;
            }
            set
            {
                checkBox6.Checked = value;
            }
        }

        private bool EveryFramePollingDisabled
        {
            get
            {
                return checkBox7.Checked;
            }
            set
            {
                checkBox7.Checked = value;
            }
        }

        private bool MenuMoviePreviewDisabled
        {
            get
            {
                return checkBox8.Checked;
            }
            set
            {
                checkBox8.Checked = value;
            }
        }

        private bool AutoAdjustLatencyDisabled
        {
            get
            {
                return checkBox11.Checked;
            }
            set
            {
                checkBox11.Checked = value;
            }
        }

        private bool ShaderDisabled
        {
            get
            {
                return checkBox4.Checked;
            }
            set
            {
                checkBox4.Checked = value;
            }
        }

        private bool FontScaleDisabled
        {
            get
            {
                return checkBox13.Checked;
            }
            set
            {
                checkBox13.Checked = value;
            }
        }

        private bool HighResolutionImageDisabled
        {
            get
            {
                return checkBox14.Checked;
            }
            set
            {
                checkBox14.Checked = value;
            }
        }

        private bool FixedFPSDisabled
        {
            get
            {
                return checkBox15.Checked;
            }
            set
            {
                checkBox15.Checked = value;
            }
        }

        private int AccurateInputSleepTime
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        private bool RunExpansion
        {
            get
            {
                return checkBox9.Checked;
            }
            set
            {
                checkBox9.Checked = value;
            }
        }

        public int ExpansionWaitPort
        {
            get
            {
                int.TryParse(textBox4.Text, out int val);
                return val;
            }
            set
            {
                textBox4.Text = value.ToString();
            }
        }

        public PPDTwitterManager TwitterManager
        {
            get
            {
                if (twitterManager == null)
                {
                    twitterManager = new PPDTwitterManager(ConsumerKey, ConsumerSecret);
                }
                return twitterManager;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var uri = TwitterManager.GetAuthorizationUri();
                Process.Start(uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Program.ErrorHandler.ProcessError(ex);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var pin = this.textBox11.Text.Trim();
                if (TwitterManager.GetTokens(pin, out string accessToken, out string accessTokenSecret))
                {
                    Token = accessToken;
                    TokenSecret = accessTokenSecret;
                    TokenAvailable = true;
                }
                else
                {
                    throw new Exception("Wrong Code!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Program.ErrorHandler.ProcessError(ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    Process.Start("http://projectdxxx.me/forum/thread/id/280");
                }
                catch
                {
                }
            }).Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ResetTwitterSetting();
        }
    }
}
