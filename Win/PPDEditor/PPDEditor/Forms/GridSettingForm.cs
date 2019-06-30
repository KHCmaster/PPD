using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class GridSettingForm : Form
    {
        string selectcolor = "...";
        delegate int Func(int val);

        public GridSettingForm()
        {
            InitializeComponent();
            this.dataGridView1.Rows.Add(new object[] { "幅", "5" });
            this.dataGridView1.Rows.Add(new object[] { "高さ", "5" });
            this.dataGridView1.Rows.Add(new object[] { "Xオフセット", "0" });
            this.dataGridView1.Rows.Add(new object[] { "Yオフセット", "0" });
            var row = new DataGridViewRow();
            var c = new DataGridViewTextBoxCell
            {
                Value = "色"
            };
            row.Cells.Add(c);
            var cell = new ExDataGridViewComboBoxCell();
            cell.SelectIndexChanged += cell_SelectIndexChanged;
            cell.Items.Add(selectcolor);
            row.Cells.Add(cell);
            this.dataGridView1.Rows.Add(row);
            cell.ColumnName = "Column2";
            cell.SetEvent();
        }

        public void SetLang()
        {
            this.dataGridView1[0, 0].Value = Utility.Language["GSFText1"];
            this.dataGridView1[0, 1].Value = Utility.Language["GSFText2"];
            this.dataGridView1[0, 2].Value = Utility.Language["GSFText3"];
            this.dataGridView1[0, 3].Value = Utility.Language["GSFText4"];
            this.dataGridView1[0, 4].Value = Utility.Language["GSFText5"];
        }

        void cell_SelectIndexChanged(DataGridViewComboBoxEditingControl cb)
        {
            if (cb.SelectedItem != null && cb.SelectedItem.ToString() == selectcolor)
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    cb.Items.Add(TranslateColor(colorDialog1.Color));
                    if (cb.Items.Count > 0) cb.SelectedIndex = cb.Items.Count - 1;
                    cb.EditingControlDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    cb.EditingControlDataGridView[1, cb.EditingControlRowIndex].Value = cb.SelectedItem.ToString();
                }
            }
        }
        public SquareGrid Grid
        {
            get
            {
                var sg = new SquareGrid
                {
                    Width = GetValue(0, 5, (X) => (X < 1 || X > 800 ? 5 : X)),
                    Height = GetValue(1, 5, (X) => (X < 1 || X > 400 ? 5 : X)),
                    OffsetX = GetValue(2, 0, (X) => (X)),
                    OffsetY = GetValue(3, 0, (X) => (X))
                };
                try
                {
                    sg.GridColor = TranslateColorString(dataGridView1[1, 4].Value.ToString());
                }
                catch
                {
                    sg.GridColor = Color.White;
                }
                return sg;
            }
            set
            {
                this.dataGridView1[1, 0].Value = value.Width;
                this.dataGridView1[1, 1].Value = value.Height;
                this.dataGridView1[1, 2].Value = value.OffsetX;
                this.dataGridView1[1, 3].Value = value.OffsetY;
                var cell = this.dataGridView1[1, 4] as DataGridViewComboBoxCell;
                cell.Items.Add(TranslateColor(value.GridColor));
                cell.Value = TranslateColor(value.GridColor);
            }
        }
        private Color TranslateColorString(string color)
        {
            return ColorTranslator.FromHtml(color);
        }
        private string TranslateColor(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
        private int GetValue(int index, int errorvalue, Func func)
        {
            if (!int.TryParse(this.dataGridView1[1, index].Value.ToString(), out int ret)) ret = errorvalue;
            return func(ret);
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
