using PPDConfiguration;

namespace PPDInstaller.Controls
{
    public partial class InstallAbortedPanel : PanelBase
    {
        public InstallAbortedPanel()
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

        public override bool CanCancel
        {
            get
            {
                return false;
            }
        }

        public override bool IsCancelVisible
        {
            get
            {
                return false;
            }
        }

        public override void SetLang(SettingReader setting)
        {
            base.SetLang(setting);
            NextText = setting["Finish"];
            this.label3.Text = setting["Label3"];
            this.label4.Text = setting["Label4"];
        }
    }
}
