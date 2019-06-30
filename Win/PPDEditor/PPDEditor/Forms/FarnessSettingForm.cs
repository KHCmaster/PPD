using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class FarnessSettingForm : Form
    {
        public FarnessSettingForm()
        {
            InitializeComponent();
            label1.DataBindings.Add("Text", trackBar1, "Value");
        }
        public float Farness
        {
            get
            {
                return (float)trackBar1.Value / trackBar1.Maximum;
            }
            set
            {
                trackBar1.Value = (int)Math.Min(value * 100, trackBar1.Maximum);
            }
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
