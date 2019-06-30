using System;
using System.Windows.Forms;

namespace PPDEditor
{
    class ExDataGridViewComboBoxCell : DataGridViewComboBoxCell
    {
        public delegate void SelectIndexChangedEventHandler(DataGridViewComboBoxEditingControl cb);
        public event SelectIndexChangedEventHandler SelectIndexChanged;
        private DataGridViewComboBoxEditingControl dataGridViewComboBox;
        public string ColumnName
        {
            get;
            set;
        }
        public void SetEvent()
        {
            this.DataGridView.CellValidating += DataGridView_CellValidating;
            this.DataGridView.EditingControlShowing += DataGridView_EditingControlShowing;
        }
        public void ResetEvent()
        {
            this.DataGridView.CellValidating -= DataGridView_CellValidating;
            this.DataGridView.EditingControlShowing -= DataGridView_EditingControlShowing;
        }

        void DataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var dgv = (DataGridView)sender;
            //該当するセルか調べる
            if (dgv.CurrentCell.ColumnIndex == ColumnIndex && dgv.CurrentCell.RowIndex == RowIndex && dgv[e.ColumnIndex, e.RowIndex] is DataGridViewComboBoxCell)
            {
                var cbc = (DataGridViewComboBoxCell)dgv[e.ColumnIndex, e.RowIndex];
                if (!cbc.Items.Contains(e.FormattedValue))
                {
                    cbc.Items.Add(e.FormattedValue);
                }
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
                //セルの値を設定しないと、元に戻ってしまう
                dgv[e.ColumnIndex, e.RowIndex].Value = e.FormattedValue;
            }
            if (this.dataGridViewComboBox != null)
            {
                this.dataGridViewComboBox.SelectedIndexChanged -= dataGridViewComboBox_SelectedIndexChanged;
                this.dataGridViewComboBox = null;
            }
        }

        void DataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                //該当する列か調べる
                var dgv = (DataGridView)sender;
                if (dgv.CurrentCell.ColumnIndex == ColumnIndex && dgv.CurrentCell.RowIndex == RowIndex && dgv.CurrentCell.OwningColumn.Name == ColumnName)
                {
                    //編集のために表示されているコントロールを取得
                    var cb =
                        (DataGridViewComboBoxEditingControl)e.Control;
                    cb.DropDownStyle = ComboBoxStyle.DropDown;
                    //編集のために表示されているコントロールを取得
                    this.dataGridViewComboBox =
                        (DataGridViewComboBoxEditingControl)e.Control;
                    //SelectedIndexChangedイベントハンドラを追加
                    this.dataGridViewComboBox.SelectedIndexChanged +=
dataGridViewComboBox_SelectedIndexChanged;

                }
            }
        }
        private void dataGridViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //選択されたアイテムを表示
            var cb = (DataGridViewComboBoxEditingControl)sender;
            if (cb != null && this.SelectIndexChanged != null)
            {
                this.SelectIndexChanged.Invoke(cb);
            }
        }
    }
}
