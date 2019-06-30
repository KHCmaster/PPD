using Effect2DEditor.DockForm;
using PPDConfiguration;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class ChangeBackGroundForm : Form
    {
        public ChangeBackGroundForm()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                this.Text = setting["CBGText"];
                this.radioButton1.Text = setting["CBGRadioButton1"];
                this.radioButton2.Text = setting["CBGRadioButton2"];
                this.radioButton3.Text = setting["CBGRadioButton3"];
            }
        }
        public BackGroundMode Mode
        {
            get
            {
                if (radioButton1.Checked) return BackGroundMode.Default;
                else if (radioButton2.Checked) return BackGroundMode.Color;
                else return BackGroundMode.Image;
            }
            set
            {
                switch (value)
                {
                    case BackGroundMode.Default:
                        radioButton1.Checked = true;
                        break;
                    case BackGroundMode.Color:
                        radioButton2.Checked = true;
                        break;
                    case BackGroundMode.Image:
                        radioButton3.Checked = true;
                        break;
                }
            }
        }
        public Color Color
        {
            get { return colorDialog1.Color; }
            set { colorDialog1.Color = value; }
        }
        public string Filename
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked |= colorDialog1.ShowDialog() == DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = openFileDialog1.FileName;
                radioButton3.Checked = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
