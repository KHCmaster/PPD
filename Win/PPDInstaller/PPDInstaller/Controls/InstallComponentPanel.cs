using PPDConfiguration;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace PPDInstaller.Controls
{
    public partial class InstallComponentPanel : PanelBase
    {
        const string PPDexepath = "Data\\PPD\\PPD.exe";
        const string BMSTOPPDexepath = "Data\\BMSTOPPD\\BMSTOPPD.exe";
        const string Effect2DEditorexepath = "Data\\Effect2DEditor\\Effect2DEditor.exe";

        [DefaultValue("")]
        public string PPDVersion { get; private set; }
        [DefaultValue("")]
        public string BMSTOPPDVersion { get; private set; }
        [DefaultValue("")]
        public string Effect2DEditorVersion { get; private set; }
        [DefaultValue("")]
        public string InstallVersion { get; private set; }

        readonly bool fontinstalled;

        public InstallComponentPanel()
        {
            InitializeComponent();
            fontinstalled = CheckFont();
            GetVersions();
        }

        public override void SetLang(SettingReader setting)
        {
            base.SetLang(setting);
            this.label5.Text = setting["Label5"];
            this.label6.Text = setting["Label6"];
            this.ppdCheckBox.Content = setting["CheckBox1"];
            this.ipaFontCheckBox.Content = setting["CheckBox4"];
            this.bmsTOPPDCheckBox.Content = setting["CheckBox6"];
            this.groupBox1.Header = setting["GroupBox1"];
            this.linkLabel1.Text = setting["LinkLabel"];
        }

        public override void OnShown(bool skip)
        {
            CheckInstallVersion();
            this.ipaFontCheckBox.IsChecked = !fontinstalled;
        }

        public InstallInfo InstallInfo
        {
            get
            {
                return new InstallInfo
                {
                    PPD = ppdCheckBox.IsChecked == true,
                    IPAFont = ipaFontCheckBox.IsChecked == true,
                    BMSTOPPD = bmsTOPPDCheckBox.IsChecked == true,
                    LAVFilters = lavFiltersCheckBox.IsChecked == true
                };
            }
        }

        private static bool CheckFont()
        {
            foreach (FontFamily ff in Fonts.SystemFontFamilies)
            {
                foreach (string name in ff.FamilyNames.Values)
                {
                    if (name == "IPAGothic" || name == "IPAゴシック")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CheckInstallVersion()
        {
            var ip = PanelManager.GetPanel<InstallPanel>();
            if (!File.Exists(ip.InstallDirectory + "PPD\\install.info")) return;
            var sr = new StreamReader(ip.InstallDirectory + "PPD\\install.info");
            var s = sr.ReadToEnd();
            sr.Close();
            var setting = new SettingReader(s);
            string temp;
            if ((temp = setting["PPDVersion"].Trim()) != "") ppdCheckBox.IsChecked = PPDVersion != temp;
            if ((temp = setting["BMSTOPPDVersion"].Trim()) != "") bmsTOPPDCheckBox.IsChecked = BMSTOPPDVersion != temp;
        }
        void GetVersions()
        {
            if (File.Exists(PPDexepath))
            {
                var fvi = FileVersionInfo.GetVersionInfo(PPDexepath);
                PPDVersion = fvi.FileVersion;
            }
            if (File.Exists(BMSTOPPDexepath))
            {
                var fvi = FileVersionInfo.GetVersionInfo(BMSTOPPDexepath);
                BMSTOPPDVersion = fvi.FileVersion;
            }
            if (File.Exists(Effect2DEditorexepath))
            {
                var fvi = FileVersionInfo.GetVersionInfo(Effect2DEditorexepath);
                Effect2DEditorVersion = fvi.FileVersion;
            }
            string location = Assembly.GetExecutingAssembly().Location;
            if (File.Exists(location))
            {
                var fvi = FileVersionInfo.GetVersionInfo(location);
                InstallVersion = fvi.FileVersion;
            }
        }

        private void LinkLabel1_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://ossipedia.ipa.go.jp/ipafont/index.html") { UseShellExecute = true });
        }
    }
}
