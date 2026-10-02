using PPDConfiguration;
using System.Windows;

namespace PPDInstaller.Controls
{
    public partial class StartPanel : PanelBase
    {
        public StartPanel()
        {
            InitializeComponent();
        }

        public override bool CanPrevious
        {
            get
            {
                return false;
            }
        }

        public override bool IsPreviousVisible
        {
            get
            {
                return false;
            }
        }

        public override bool CanNext
        {
            get
            {
                return false;
            }
        }

        public override bool IsNextVisible
        {
            get
            {
                return false;
            }
        }

        public override void SetLang(SettingReader setting)
        {
            base.SetLang(setting);
            this.label1.Text = setting["Label1"];
            this.button1.Content = setting["EasyInstall"];
            this.button2.Content = setting["CustomInstall"];
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            while (PanelManager.CurrentPanel is not ConfirmPanel)
            {
                PanelManager.Next();
            }
            PanelManager.Next();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            PanelManager.Next();
        }
    }
}
