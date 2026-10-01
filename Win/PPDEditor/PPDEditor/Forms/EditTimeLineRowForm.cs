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
            rows = [.. rowManager.RowOrders.Select(row => new RowItem(row, rowManager.RowVisibilities[row]))];
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
            rowManager.RowOrders = [.. rows.Select(r => r.RowIndex)];
            rowManager.RowVisibilities = [.. rows.OrderBy(r => r.RowIndex).Select(r => r.IsVisible)];
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Button3_Click(object sender, EventArgs e)
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
            (rows[rowIndex], rows[rowIndex - 1]) = (rows[rowIndex - 1], rows[rowIndex]);
            dataGridView1.Invalidate();
            dataGridView1.Rows[rowIndex - 1].Selected = true;
        }

        private void Button4_Click(object sender, EventArgs e)
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
            (rows[rowIndex], rows[rowIndex + 1]) = (rows[rowIndex + 1], rows[rowIndex]);
            dataGridView1.Invalidate();
            dataGridView1.Rows[rowIndex + 1].Selected = true;
        }

        class RowItem(int rowIndex, bool isVisible)
        {
            public int RowIndex
            {
                get;
                private set;
            } = rowIndex;

            public bool IsVisible
            {
                get;
                set;
            } = isVisible;

            public string Name
            {
                get
                {
                    return Utility.Language[((ButtonType)RowIndex).ToString()];
                }
            }
        }
    }
}
