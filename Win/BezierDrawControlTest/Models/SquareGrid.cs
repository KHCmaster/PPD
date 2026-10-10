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
            // ずらした分を引いてから一番近い格子点を求める（以前はずらす前に丸めていたので、ずれが半分より大きいときや負の座標で、違う格子点になっていた）
            var nearest = new PointF(MathF.Floor((point.X - NormalizedOffsetX) / Width + 0.5f) * Width + NormalizedOffsetX,
                MathF.Floor((point.Y - NormalizedOffsetY) / Height + 0.5f) * Height + NormalizedOffsetY);
            var diffX = nearest.X - point.X;
            var diffY = nearest.Y - point.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY) >= snapDistance ? point : nearest;
        }
    }
}
