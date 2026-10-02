using PPDConfiguration;

namespace PPDInstaller.Controls
{
    public partial class LinkCreatePanel : PanelBase
    {
        public LinkCreatePanel()
        {
            InitializeComponent();
        }

        public override void SetLang(SettingReader setting)
        {
            base.SetLang(setting);
            registerStartMenuCheckBox.Content = setting["RegisterStartMenu"];
        }

        public bool CreateStartMenu
        {
            get
            {
                return registerStartMenuCheckBox.IsChecked == true;
            }
        }
    }
}
