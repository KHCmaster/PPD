using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor.Controls
{
    /// <summary>
    /// 左右にドラッグすると、動かした量を通知するラベルです（WinForms の <see cref="ChangeValueLabel"/> の WPF 版）。
    /// </summary>
    public class DragValueLabel : TextBlock
    {
        public event ValueChangeEventHandler ValueChange;
        double lastx;
        double remainder;

        public DragValueLabel()
        {
            Cursor = Cursors.SizeWE;
            // 文字のないところでもドラッグできるように、背景を透明で塗る
            Background = System.Windows.Media.Brushes.Transparent;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            lastx = e.GetPosition(this).X;
            remainder = 0;
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!IsMouseCaptured)
            {
                return;
            }
            var x = e.GetPosition(this).X;
            remainder += x - lastx;
            lastx = x;
            var changed = (int)remainder;
            if (changed != 0)
            {
                remainder -= changed;
                ValueChange?.Invoke(this, new ValueChangeEventArgs
                {
                    ChangedValue = changed
                });
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
        }
    }
}
