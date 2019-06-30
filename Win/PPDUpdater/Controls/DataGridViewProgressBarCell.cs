using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDUpdater.Controls
{
    /// <summary>
    /// ProgressBarをDataGridViewに表示する
    /// </summary>
    public class DataGridViewProgressBarCell : DataGridViewTextBoxCell
    {
        //コンストラクタ
        public DataGridViewProgressBarCell()
        {
            this.maximumValue = 100;
            this.mimimumValue = 0;
        }

        private int maximumValue;
        public int Maximum
        {
            get
            {
                return this.maximumValue;
            }
            set
            {
                this.maximumValue = value;
            }
        }

        private int mimimumValue;
        public int Mimimum
        {
            get
            {
                return this.mimimumValue;
            }
            set
            {
                this.mimimumValue = value;
            }
        }

        //セルの値のデータ型を指定する
        //ここでは、整数型とする
        public override Type ValueType
        {
            get
            {
                return typeof(int);
            }
        }

        //新しいレコード行のセルの既定値を指定する
        public override object DefaultNewRowValue
        {
            get
            {
                return 0;
            }
        }

        //新しいプロパティを追加しているため、
        // Cloneメソッドをオーバーライドする必要がある
        public override object Clone()
        {
            var cell =
                (DataGridViewProgressBarCell)base.Clone();
            cell.Maximum = this.Maximum;
            cell.Mimimum = this.Mimimum;
            return cell;
        }

        protected override void Paint(Graphics graphics,
            Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates cellState,
            object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            //値を決定する
            int intValue = 0;
            if (value is int)
                intValue = (int)value;
            if (intValue < this.mimimumValue)
                intValue = this.mimimumValue;
            if (intValue > this.maximumValue)
                intValue = this.maximumValue;
            //割合を計算する
            double rate = (double)(intValue - this.mimimumValue) /
                (this.maximumValue - this.mimimumValue);

            //セルの境界線（枠）を描画する
            if ((paintParts & DataGridViewPaintParts.Border) ==
                DataGridViewPaintParts.Border)
            {
                this.PaintBorder(graphics, clipBounds, cellBounds,
                    cellStyle, advancedBorderStyle);
            }

            //境界線の内側に範囲を取得する
            var borderRect = this.BorderWidths(advancedBorderStyle);
            var paintRect = new Rectangle(
                cellBounds.Left + borderRect.Left,
                cellBounds.Top + borderRect.Top,
                cellBounds.Width - borderRect.Right,
                cellBounds.Height - borderRect.Bottom);

            //背景色を決定する
            //選択されている時とされていない時で色を変える
            bool isSelected =
                (cellState & DataGridViewElementStates.Selected) ==
                DataGridViewElementStates.Selected;
            Color bkColor;
            if (isSelected &&
                (paintParts & DataGridViewPaintParts.SelectionBackground) ==
                    DataGridViewPaintParts.SelectionBackground)
            {
                bkColor = cellStyle.SelectionBackColor;
            }
            else
            {
                bkColor = cellStyle.BackColor;
            }
            //背景を描画する
            if ((paintParts & DataGridViewPaintParts.Background) ==
                DataGridViewPaintParts.Background)
            {
                using (SolidBrush backBrush = new SolidBrush(bkColor))
                {
                    graphics.FillRectangle(backBrush, paintRect);
                }
            }

            //Paddingを差し引く
            paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
            paintRect.Width -= cellStyle.Padding.Horizontal;
            paintRect.Height -= cellStyle.Padding.Vertical;

            //ProgressBarを描画する
            if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                DataGridViewPaintParts.ContentForeground)
            {
                if (ProgressBarRenderer.IsSupported)
                {
                    //visualスタイルで描画する

                    //ProgressBarの枠を描画する
                    ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);
                    //ProgressBarのバーを描画する
                    var barBounds = new Rectangle(
                        paintRect.Left + 3, paintRect.Top + 3,
                        paintRect.Width - 4, paintRect.Height - 6);
                    barBounds.Width = (int)Math.Round(barBounds.Width * rate);
                    ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);
                }
                else
                {
                    //visualスタイルで描画できない時

                    graphics.FillRectangle(Brushes.White, paintRect);
                    graphics.DrawRectangle(Pens.Black, paintRect);
                    var barBounds = new Rectangle(
                        paintRect.Left + 1, paintRect.Top + 1,
                        paintRect.Width - 1, paintRect.Height - 1);
                    barBounds.Width = (int)Math.Round(barBounds.Width * rate);
                    graphics.FillRectangle(Brushes.Blue, barBounds);
                }
            }

            //フォーカスの枠を表示する
            if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex &&
                this.DataGridView.CurrentCellAddress.Y == this.RowIndex &&
                (paintParts & DataGridViewPaintParts.Focus) ==
                    DataGridViewPaintParts.Focus &&
                this.DataGridView.Focused)
            {
                //フォーカス枠の大きさを適当に決める
                Rectangle focusRect = paintRect;
                focusRect.Inflate(-3, -3);
                ControlPaint.DrawFocusRectangle(graphics, focusRect);
                //背景色を指定してフォーカス枠を描画する時
                //ControlPaint.DrawFocusRectangle(
                //    graphics, focusRect, Color.Empty, bkColor);
            }

            //テキストを表示する
            if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                DataGridViewPaintParts.ContentForeground)
            {
                //表示するテキストを決定
                var txt = string.Format("{0}%", Math.Round(rate * 100));
                //string txt = formattedValue.ToString();

                //本来は、cellStyleによりTextFormatFlagsを決定すべき
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter;
                //色を決定
                Color fColor = cellStyle.ForeColor;
                //if (isSelected)
                //    fColor = cellStyle.SelectionForeColor;
                //else
                //    fColor = cellStyle.ForeColor;
                //テキストを描画する
                paintRect.Inflate(-2, -2);
                TextRenderer.DrawText(graphics, txt, cellStyle.Font,
                    paintRect, fColor, flags);
            }

            //エラーアイコンの表示
            if ((paintParts & DataGridViewPaintParts.ErrorIcon) ==
                DataGridViewPaintParts.ErrorIcon &&
                this.DataGridView.ShowCellErrors &&
                !string.IsNullOrEmpty(errorText))
            {
                //エラーアイコンを表示させる領域を取得
                var iconBounds = this.GetErrorIconBounds(
                    graphics, cellStyle, rowIndex);
                iconBounds.Offset(cellBounds.X, cellBounds.Y);
                //エラーアイコンを描画
                this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
            }
        }
    }
}
