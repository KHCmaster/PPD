using PPDConfiguration;
using System;

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
            this.button1.Text = setting["EasyInstall"];
            this.button2.Text = setting["CustomInstall"];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            while (!(PanelManager.CurrentPanel is ConfirmPanel))
            {
                PanelManager.Next();
            }
            PanelManager.Next();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PanelManager.Next();
        }
    }
}
