using PPDConfiguration;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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

        bool fontinstalled;

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
            this.ppdCheckBox.Text = setting["CheckBox1"];
            this.ipaFontCheckBox.Text = setting["CheckBox4"];
            this.bmsTOPPDCheckBox.Text = setting["CheckBox6"];
            this.groupBox1.Text = setting["GroupBox1"];
            this.linkLabel1.Text = setting["LinkLabel"];
        }

        public override void OnShown(bool skip)
        {
            CheckInstallVersion();
            this.ipaFontCheckBox.Checked = !fontinstalled;
        }

        public InstallInfo InstallInfo
        {
            get
            {
                return new InstallInfo
                {
                    PPD = ppdCheckBox.Checked,
                    IPAFont = ipaFontCheckBox.Checked,
                    BMSTOPPD = bmsTOPPDCheckBox.Checked,
                    LAVFilters = lavFiltersCheckBox.Checked
                };
            }
        }

        private bool CheckFont()
        {
            var ifc =
                new System.Drawing.Text.InstalledFontCollection();
            FontFamily[] ffs = ifc.Families;
            foreach (FontFamily ff in ffs)
            {
                if (ff.GetName(0) == "IPAGothic" || ff.GetName(0) == "IPAゴシック")
                {
                    return true;
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
            string temp = "";
            if ((temp = setting["PPDVersion"].Trim()) != "") ppdCheckBox.Checked = PPDVersion != temp;
            if ((temp = setting["BMSTOPPDVersion"].Trim()) != "") bmsTOPPDCheckBox.Checked = BMSTOPPDVersion != temp;
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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ossipedia.ipa.go.jp/ipafont/index.html");
        }
    }
}
