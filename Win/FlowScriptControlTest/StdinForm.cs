using System;
using System.Windows.Forms;

namespace FlowScriptControlTest
{
    public partial class StdinForm : Form
    {
        public string InputText
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public StdinForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
