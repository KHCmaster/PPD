using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class SuggestForm : Form
    {
        public string SelectedName
        {
            get;
            private set;
        }

        public string SelectedValue
        {
            get;
            private set;
        }

        public SuggestForm()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.Text = Utility.Language["Suggestion"];
            this.Column1.HeaderText = Utility.Language["Name"];
            this.Column2.HeaderText = Utility.Language["Value"];
        }

        public void AddSuggest(string name, string value)
        {
            dataGridView1.Rows.Add(name, value);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SelectedName = dataGridView1[0, dataGridView1.SelectedRows[0].Index].Value.ToString();
            SelectedValue = dataGridView1[1, dataGridView1.SelectedRows[0].Index].Value.ToString();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            okButton.Enabled = dataGridView1.SelectedRows.Count == 1;
        }
    }
}
