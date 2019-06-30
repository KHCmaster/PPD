using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class ChangeKasiForm : Form
    {
        public ChangeKasiForm()
        {
            InitializeComponent();
        }
        public void SetLang()
        {
            this.Text = Utility.Language["KEButton4"];
            this.label1.Text = Utility.Language["KEDialogLabel1"];
            this.label2.Text = Utility.Language["KEDialogLabel2"];
            this.label3.Text = Utility.Language["KEDialogLabel3"];
        }
        public string beforetext
        {
            set
            {
                this.textBox1.Text = value;
            }
        }
        public string newtext
        {
            get
            {
                return this.textBox2.Text;
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
