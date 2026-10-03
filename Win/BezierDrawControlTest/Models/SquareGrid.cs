using System;
using System.Drawing;

namespace BezierDrawControlTest.Models
{
    /// <summary>
    /// 正方形の格子です。点を近くの格子点に吸い付けます。
    /// </summary>
    public class SquareGrid
    {
        public SquareGrid()
        {
            Width = 5;
            Height = 5;
            OffsetX = 0;
            OffsetY = 0;
            GridColor = ColorTranslator.FromHtml("#FFFFFF");
        }
        public int Width
        {
            get;
            set;
        }
        public int Height
        {
            get;
            set;
        }
        public int OffsetX
        {
            get;
            set;
        }
        public int OffsetY
        {
            get;
            set;
        }
        public Color GridColor
        {
            get;
            set;
        }
        public int NormalizedOffsetX
        {
            get
            {
                return Normalize(OffsetX, Width);
            }
        }
        public int NormalizedOffsetY
        {
            get
            {
                return Normalize(OffsetY, Height);
            }
        }
        private static int Normalize(int num, int basenum)
        {
            int val = num % basenum;
            if (val < 0) val += basenum;
            return val;
        }

        /// <summary>
        /// 格子点から snapDistance より近い点は、格子点に移します。
        /// </summary>
        public PointF Snap(PointF point, float snapDistance)
        {
            var nearest = new PointF((int)(point.X + Width / 2) / Width * Width + NormalizedOffsetX,
                (int)(point.Y + Height / 2) / Height * Height + NormalizedOffsetY);
            var diffX = nearest.X - point.X;
            var diffY = nearest.Y - point.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY) >= snapDistance ? point : nearest;
        }
    }
}
