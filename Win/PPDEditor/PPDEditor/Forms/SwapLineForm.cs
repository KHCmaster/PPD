using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class SwapLineForm : Form
    {
        public void SetLang()
        {
            this.Text = Utility.Language["SwapLine"];
            this.label1.Text = Utility.Language["SwapLineLabel1"];
            this.label2.Text = Utility.Language["SwapLineLabel2"];
            this.label3.Text = Utility.Language["SwapLineLabel3"];
            this.comboBox1.Items.Clear();
            this.comboBox2.Items.Clear();
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox1"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox2"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox3"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox4"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox5"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox6"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox7"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox8"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox9"]);
            this.comboBox1.Items.Add(Utility.Language["SwapLineBox10"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox1"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox2"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox3"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox4"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox5"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox6"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox7"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox8"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox9"]);
            this.comboBox2.Items.Add(Utility.Language["SwapLineBox10"]);
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
        }
        public int first
        {
            get
            {
                return this.comboBox1.SelectedIndex;
            }
        }
        public int second
        {
            get
            {
                return this.comboBox2.SelectedIndex;
            }
        }
        public SwapLineForm()
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
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
