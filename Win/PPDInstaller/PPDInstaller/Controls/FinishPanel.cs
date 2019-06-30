using PPDConfiguration;

namespace PPDInstaller.Controls
{
    public partial class FinishPanel : PanelBase
    {
        public FinishPanel()
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
            this.label8.Text = setting["Label8"];
            this.label9.Text = setting["Label9"];
        }

        public string ErrorText
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.textBox2.Text = value;
                    this.label9.Visible = true;
                    textBox2.Visible = true;
                }
            }
        }
    }
}
