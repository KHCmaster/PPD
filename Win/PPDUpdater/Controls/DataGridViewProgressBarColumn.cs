using System;
using System.Windows.Forms;

namespace PPDUpdater.Controls
{
    /// <summary>
    /// DataGridViewProgressBarCellオブジェクトの列
    /// </summary>
    public class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn
    {
        //コンストラクタ
        public DataGridViewProgressBarColumn()
        {
            this.CellTemplate = new DataGridViewProgressBarCell();
        }

        //CellTemplateの取得と設定
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                //DataGridViewProgressBarCell以外はホストしない
                if (!(value is DataGridViewProgressBarCell))
                {
                    throw new InvalidCastException(
                        "DataGridViewProgressBarCellオブジェクトを" +
                        "指定してください。");
                }
                base.CellTemplate = value;
            }
        }

        /// <summary>
        /// ProgressBarの最大値
        /// </summary>
        public int Maximum
        {
            get
            {
                return ((DataGridViewProgressBarCell)this.CellTemplate).Maximum;
            }
            set
            {
                if (this.Maximum == value)
                    return;
                //セルテンプレートの値を変更する
                ((DataGridViewProgressBarCell)this.CellTemplate).Maximum =
                    value;
                //DataGridViewにすでに追加されているセルの値を変更する
                if (this.DataGridView == null)
                    return;
                int rowCount = this.DataGridView.RowCount;
                for (int i = 0; i < rowCount; i++)
                {
                    var r = this.DataGridView.Rows.SharedRow(i);
                    ((DataGridViewProgressBarCell)r.Cells[this.Index]).Maximum =
                        value;
                }
            }
        }

        /// <summary>
        /// ProgressBarの最小値
        /// </summary>
        public int Mimimum
        {
            get
            {
                return ((DataGridViewProgressBarCell)this.CellTemplate).Mimimum;
            }
            set
            {
                if (this.Mimimum == value)
                    return;
                //セルテンプレートの値を変更する
                ((DataGridViewProgressBarCell)this.CellTemplate).Mimimum =
                    value;
                //DataGridViewにすでに追加されているセルの値を変更する
                if (this.DataGridView == null)
                    return;
                int rowCount = this.DataGridView.RowCount;
                for (int i = 0; i < rowCount; i++)
                {
                    var r = this.DataGridView.Rows.SharedRow(i);
                    ((DataGridViewProgressBarCell)r.Cells[this.Index]).Mimimum =
                        value;
                }
            }
        }
    }

}
