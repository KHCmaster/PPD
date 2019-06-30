using PPDConfiguration;
using System;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class CanvasSizeForm : Form
    {
        public CanvasSizeForm()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                this.Text = setting["CSFText"];
                this.label1.Text = setting["CSFLabel1"];
                this.label2.Text = setting["CSFLabel2"];
            }
        }
        public int CanvasWidth
        {
            get
            {
                if (!int.TryParse(this.textBox1.Text, out int val)) val = 256;
                return val;
            }
            set
            {
                this.textBox1.Text = value.ToString();
            }
        }
        public int CanvasHeight
        {
            get
            {
                if (!int.TryParse(this.textBox2.Text, out int val)) val = 256;
                return val;
            }
            set
            {
                this.textBox2.Text = value.ToString();
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
