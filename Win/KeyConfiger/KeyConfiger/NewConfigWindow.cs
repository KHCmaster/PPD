using System;
using System.Windows.Forms;

namespace KeyConfiger
{
    public partial class NewConfigWindow : Form
    {
        public NewConfigWindow()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.label1.Text = Utility.Language["NCWLabel1"];
            this.label2.Text = Utility.Language["NCWLabel2"];
            this.Text = Utility.Language["CreateNewSetting"];
        }

        public string SettingName
        {
            get
            {
                return textBox1.Text;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.TextLength > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
