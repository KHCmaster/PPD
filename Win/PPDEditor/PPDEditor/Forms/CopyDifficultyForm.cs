using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class CopyDifficultyForm : Form
    {
        public CopyDifficultyForm()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.Text = Utility.Language["CopyDifficulty"];
            this.label1.Text = Utility.Language["CDFLabel1"];
            this.label2.Text = Utility.Language["CDFLabel2"];
        }

        public AvailableDifficulty CurrentDifficulty
        {
            set
            {
                comboBox1.Items.Clear();
                foreach (AvailableDifficulty difficulty in EditorForm.DifficultyArray)
                {
                    if (difficulty != value && difficulty != AvailableDifficulty.None)
                    {
                        comboBox1.Items.Add(difficulty.ToString());
                    }
                }
                comboBox1.SelectedIndex = 0;
                label3.Text = value.ToString();
            }
        }

        public AvailableDifficulty DestDifficulty
        {
            get
            {
                return EditorForm.GetDifficultyFromString(comboBox1.SelectedItem.ToString());
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
