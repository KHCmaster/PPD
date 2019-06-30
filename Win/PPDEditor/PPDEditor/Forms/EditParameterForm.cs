using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class EditParameterForm : Form
    {
        HashSet<string> existKeys;

        public Dictionary<string, string> Parameters
        {
            get
            {
                return GetParametersExceptRow(-1);
            }
            set
            {
                dataGridView1.RowCount = 0;
                foreach (var pair in value)
                {
                    dataGridView1.Rows.Add(pair.Key, pair.Value);
                }
            }
        }

        public string[] ExistKeys
        {
            set
            {
                existKeys = new HashSet<string>(value);
            }
        }

        public EditParameterForm()
        {
            InitializeComponent();
            existKeys = new HashSet<string>();
        }

        private Dictionary<string, string> GetParametersExceptRow(int rowIndex)
        {
            var ret = new Dictionary<string, string>();
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (i == rowIndex)
                {
                    continue;
                }
                ret.Add(dataGridView1[0, i].Value.ToString(), dataGridView1[1, i].Value.ToString());
            }
            return ret;
        }

        public void SetLang()
        {
            Text = Utility.Language["EditParameter"];
            KeyColumn.HeaderText = Utility.Language["Key"];
            ValueColumn.HeaderText = Utility.Language["Value"];
            addButton.Text = Utility.Language["Add"];
            removeButton.Text = Utility.Language["Remove"];
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var parameters = Parameters;
            string name = null;
            int index = 1;
            while (true)
            {
                name = String.Format("Key{0}", index);
                if (!parameters.ContainsKey(name) && !existKeys.Contains(name))
                {
                    break;
                }
                index++;
            }
            dataGridView1.Rows.Add(name, "");
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                return;
            }

            int[] rows = new int[dataGridView1.SelectedRows.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = dataGridView1.SelectedRows[i].Index;
            }
            foreach (var index in rows.OrderByDescending(i => i))
            {
                dataGridView1.Rows.RemoveAt(index);
            }
        }

        private void EditParameterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                var keys = new HashSet<string>();
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    var key = dataGridView1[0, i].Value.ToString();
                    if (keys.Contains(key))
                    {
                        MessageBox.Show(Utility.Language["DuplicateKeyError"]);
                        e.Cancel = true;
                        return;
                    }
                    keys.Add(key);
                }
            }
        }
    }
}
