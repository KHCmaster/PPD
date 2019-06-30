using System;
using System.Windows.Forms;

namespace PPDInstaller
{
    public partial class LangSelectForm : Form
    {
        public LangSelectForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void SetLang(string[] langs)
        {
            foreach (var s in langs)
            {
                comboBox1.Items.Add(s);
            }
            comboBox1.SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get
            {
                return comboBox1.SelectedIndex;
            }
        }
    }
}
