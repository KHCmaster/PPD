using System;
using System.IO;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class SavePosAndAngleForm : Form
    {
        string NameError1 = "名称には何かを入力してください";
        string NameError2 = "入力された名称はすでに使用されております";

        public bool WithAngle
        {
            get
            {
                return radioButton2.Checked;
            }
        }

        public string FileName
        {
            get;
            private set;
        }

        public SavePosAndAngleForm(string text)
        {
            InitializeComponent();
            this.label2.Text = text;
        }

        public void SetLang()
        {
            this.label1.Text = Utility.Language["PAALSDialogLabel1"];
            this.radioButton1.Text = Utility.Language["PAALSDialogRadio1"];
            this.radioButton2.Text = Utility.Language["PAALSDialogRadio2"];
            NameError1 = Utility.Language["PAALSNameError1"];
            NameError2 = Utility.Language["PAALSNameError2"];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileName = this.textBox1.Text;
            if (FileName == "")
            {
                MessageBox.Show(NameError1);
            }
            else if (File.Exists("posdat\\" + FileName + ".txt"))
            {
                MessageBox.Show(NameError2);
                this.textBox1.Focus();
                this.textBox1.SelectAll();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.IsInputKey = true;
                button1_Click(sender, e);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                e.Handled = true;
            }
        }
    }
}
