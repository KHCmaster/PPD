using PPDEditor.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor
{
    class CustomToolStripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is ExToolStripSplitButton item && item.Checked)
            {
                DrawItemCheck(new ToolStripItemImageRenderEventArgs(e.Graphics, item, new Rectangle(2, 3, item.ContentRectangle.Width, item.ContentRectangle.Height)));
                DrawArrow(new ToolStripArrowRenderEventArgs(
                    e.Graphics,
                    item,
                    item.DropDownButtonBounds,
                    SystemColors.ControlText,
                    ArrowDirection.Down));
                var p = new Pen(this.ColorTable.ButtonSelectedBorder);
                e.Graphics.DrawLine(p, new Point(item.DropDownButtonBounds.Left, item.DropDownButtonBounds.Top + 2), new Point(item.DropDownButtonBounds.Left, item.DropDownButtonBounds.Bottom - 2));
            }
            else
            {
                base.OnRenderSplitButtonBackground(e);
            }
        }
    }
}
