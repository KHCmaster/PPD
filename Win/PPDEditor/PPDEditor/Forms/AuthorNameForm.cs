using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class AuthorNameForm : Form
    {
        public AuthorNameForm()
        {
            InitializeComponent();
        }
        public void SetLang()
        {
            this.label1.Text = Utility.Language["AuthorNameLabel"];
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        public string AuthorName
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }
    }
}
