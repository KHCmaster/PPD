using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace BezierDrawControl
{
    public static class MyCursors
    {
        static readonly Dictionary<int, Cursor> dict = [];
        public static readonly Cursor Rotate = new(BezierDrawControl.Properties.Resources.rotate.GetHicon());
        public static readonly Cursor Anchor = new(BezierDrawControl.Properties.Resources.anchor.GetHicon());
        public static readonly Cursor AnchorPlus = new(BezierDrawControl.Properties.Resources.anchorplus.GetHicon());
        public static readonly Cursor Handle = new(BezierDrawControl.Properties.Resources.handle.GetHicon());

        /// <summary>
        /// <see cref="BezierCursor"/> を WinForms のカーソルにします。
        /// </summary>
        public static Cursor Get(BezierCursor cursor)
        {
            return cursor.Kind switch
            {
                BezierCursorKind.SizeAll => Cursors.SizeAll,
                BezierCursorKind.Rotate => Rotate,
                BezierCursorKind.Anchor => Anchor,
                BezierCursorKind.AnchorPlus => AnchorPlus,
                BezierCursorKind.Handle => Handle,
                BezierCursorKind.Arrow => CreateArrowCursor(cursor.Angle),
                _ => Cursors.Default,
            };
        }

        /// <summary>
        /// WinForms の修飾キーを <see cref="BezierModifierKeys"/> にします。
        /// </summary>
        public static BezierModifierKeys ToBezierModifierKeys(Keys keys)
        {
            var result = BezierModifierKeys.None;
            if ((keys & Keys.Shift) == Keys.Shift) result |= BezierModifierKeys.Shift;
            if ((keys & Keys.Control) == Keys.Control) result |= BezierModifierKeys.Control;
            if ((keys & Keys.Alt) == Keys.Alt) result |= BezierModifierKeys.Alt;
            return result;
        }

        public static Cursor CreateArrowCursor(int angle)
        {
            angle = NormalizeAngle(angle);
            if (dict.TryGetValue(angle, out Cursor value))
            {
                return value;
            }
            using Bitmap bit = new(32, 32);
            using Graphics g = Graphics.FromImage(bit);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TranslateTransform(16, 16);
            g.RotateTransform(angle);
            var points = new PointF[]{
                    new(8,0),
                    new(5,3),
                    new(5,1),
                    new(-5,1),
                    new(-5,3),
                    new(-8,0),
                    new(-5,-3),
                    new(-5,-1),
                    new(5,-1),
                    new(5,-3)
                };
            g.FillPolygon(Brushes.Black, points);
            var cursor = new Cursor(bit.GetHicon());
            dict[angle] = cursor;
            return cursor;
        }

        private static int NormalizeAngle(int num)
        {
            num %= 180;
            if (num < 0) num += 180;
            return num;
        }
    }
}
