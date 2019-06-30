using PPDConfiguration;
using System;
using System.Windows.Forms;

namespace PPDInstaller.Controls
{
    public partial class InstallPanel : PanelBase
    {
        public InstallPanel()
        {
            InitializeComponent();
            if (System.Environment.SystemDirectory.Length >= 2)
            {
                this.textBox1.Text = System.Environment.SystemDirectory.Substring(0, 2) + "\\KHC";
            }
            else
            {
                if (System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).Length >= 2)
                {
                    this.textBox1.Text = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).Substring(0, 2) + "\\KHC";
                }
            }
        }

        public override void SetLang(SettingReader setting)
        {
            base.SetLang(setting);
            this.label2.Text = setting["Label2"];
        }

        public string InstallDirectory
        {
            get
            {
                return this.textBox1.Text + "\\";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = this.textBox1.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) this.textBox1.Text = folderBrowserDialog1.SelectedPath + "\\KHC";
        }
    }
}
