using PPDFramework;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class EditTimeLineRowForm : Form
    {
        private RowItem[] rows;

        public EditTimeLineRowForm()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
        }

        public void SetRowManager(TimeLineRowManager rowManager)
        {
            rows = rowManager.RowOrders.Select(row => new RowItem(row, rowManager.RowVisibilities[row])).ToArray();
            dataGridView1.DataSource = rows;
        }

        public void SetLang()
        {
            Column1.HeaderText = Utility.Language["Kind"];
            Column2.HeaderText = Utility.Language["Visible"];
            button3.Text = Utility.Language["MoveUp"];
            button4.Text = Utility.Language["MoveDown"];
        }

        public void UpdateRowManager(TimeLineRowManager rowManager)
        {
            rowManager.RowOrders = rows.Select(r => r.RowIndex).ToArray();
            rowManager.RowVisibilities = rows.OrderBy(r => r.RowIndex).Select(r => r.IsVisible).ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                return;
            }

            var rowIndex = dataGridView1.SelectedRows[0].Index;
            if (rowIndex <= 0)
            {
                return;
            }
            var temp = rows[rowIndex - 1];
            rows[rowIndex - 1] = rows[rowIndex];
            rows[rowIndex] = temp;
            dataGridView1.Invalidate();
            dataGridView1.Rows[rowIndex - 1].Selected = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                return;
            }

            var rowIndex = dataGridView1.SelectedRows[0].Index;
            if (rowIndex >= rows.Length - 1)
            {
                return;
            }
            var temp = rows[rowIndex + 1];
            rows[rowIndex + 1] = rows[rowIndex];
            rows[rowIndex] = temp;
            dataGridView1.Invalidate();
            dataGridView1.Rows[rowIndex + 1].Selected = true;
        }

        class RowItem
        {
            public int RowIndex
            {
                get;
                private set;
            }

            public bool IsVisible
            {
                get;
                set;
            }

            public string Name
            {
                get
                {
                    return Utility.Language[((ButtonType)RowIndex).ToString()];
                }
            }

            public RowItem(int rowIndex, bool isVisible)
            {
                RowIndex = rowIndex;
                IsVisible = isVisible;
            }
        }
    }
}
