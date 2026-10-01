using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace BezierDrawControl
{
    public static class MyCursors
    {
        static readonly Dictionary<int, Cursor> dict = [];
        public static Cursor Rotate = new(BezierDrawControl.Properties.Resources.rotate.GetHicon());
        public static Cursor Anchor = new(BezierDrawControl.Properties.Resources.anchor.GetHicon());
        public static Cursor AnchorPlus = new(BezierDrawControl.Properties.Resources.anchorplus.GetHicon());
        public static Cursor Handle = new(BezierDrawControl.Properties.Resources.handle.GetHicon());

        public static Cursor CreateArrowCursor(int angle)
        {
            angle = NormalizeAngle(angle);
            if (dict.ContainsKey(angle))
            {
                return dict[angle];
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
