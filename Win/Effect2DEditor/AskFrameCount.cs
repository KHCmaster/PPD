using PPDConfiguration;
using System;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class AskFrameCount : Form
    {
        public AskFrameCount()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                this.Text = setting["AFCText"];
                this.label1.Text = setting["AFCLabel1"];
                this.label2.Text = setting["AFCLabel2"];
                this.checkBox1.Text = setting["AFCCheckBox1"];
            }
        }
        public int FrameCount
        {
            get
            {
                if (!int.TryParse(this.textBox1.Text, out int val)) val = 1;
                if (val < 1) val = 1;
                return val;
            }
        }
        public bool WithReverse
        {
            get
            {
                return checkBox1.Checked;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
