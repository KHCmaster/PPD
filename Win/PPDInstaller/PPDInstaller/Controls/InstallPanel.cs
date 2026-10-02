using PPDConfiguration;
using Microsoft.Win32;
using System.Windows;

namespace PPDInstaller.Controls
{
    public partial class InstallPanel : PanelBase
    {
        public InstallPanel()
        {
            InitializeComponent();
            if (System.Environment.SystemDirectory.Length >= 2)
            {
                this.textBox1.Text = System.Environment.SystemDirectory[..2] + "\\KHC";
            }
            else
            {
                if (System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).Length >= 2)
                {
                    this.textBox1.Text = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)[..2] + "\\KHC";
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

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                InitialDirectory = this.textBox1.Text
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == true) this.textBox1.Text = dialog.FolderName + "\\KHC";
        }
    }
}
