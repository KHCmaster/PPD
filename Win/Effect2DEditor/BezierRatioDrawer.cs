using BezierCaliculator;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointF = System.Drawing.PointF;

namespace Effect2DEditor
{
    /// <summary>
    /// 変化の割合（0 から 128 のベジエ曲線）を編集する領域です。
    /// </summary>
    public class BezierRatioDrawer : DrawingSurface
    {
        static readonly Pen anchorPen = CreateAnchorPen();
        private BezierControlPoint selected;
        private BezierControlPoint[] bcps;

        public BezierRatioDrawer()
        {
            Background = Brushes.White;
            Render = Draw;
            // ベジエ曲線はなめらかに描く
            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
            SetLinearRatio();
        }

        private static Pen CreateAnchorPen()
        {
            var pen = new Pen(Brushes.Blue, 2);
            pen.Freeze();
            return pen;
        }

        public BezierControlPoint[] BCPS
        {
            get { return bcps; }
            set
            {
                bcps = value;
                IsLinear = false;
            }
        }
        public void SetLinearRatio()
        {
            bcps = [
                BezierControlPoint.Deserialize("VT=1 SX=0 SY=128 TDX=0.707106781 TDY=-0.707106781 TL=60"),
                BezierControlPoint.Deserialize("VF=1 SX=128 SY=0 FDX=-0.707106781 FDY=0.707106781 FL=60")
            ];
            IsLinear = true;
            DrawandRefresh();
        }
        public bool IsLinear
        {
            get;
            private set;
        }
        private void Draw(DrawingContext g)
        {
            if (bcps.Length == 2)
            {
                DrawNormalData(g);
            }
        }
        private void DrawNormalData(DrawingContext g)
        {
            BezierControlPoint p1 = bcps[0];
            BezierControlPoint p2 = bcps[1];
            DrawBezeier(p1.Second, p1.Third, p2.First, p2.Second, g);
            DrawAnchor(p1, g);
            DrawAnchor(p2, g);
        }
        private static void DrawBezeier(PointF p1, PointF p2, PointF p3, PointF p4, DrawingContext g)
        {
            PointF lastbezeirpoint, bezeirpoint;
            lastbezeirpoint = p1;
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                bezeirpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                g.DrawLine(BlackPen, ToPoint(lastbezeirpoint), ToPoint(bezeirpoint));
                lastbezeirpoint = bezeirpoint;
            }
        }
        private static void DrawAnchor(BezierControlPoint bcp, DrawingContext g)
        {
            if (bcp.ValidFirst)
            {
                FillSquare(bcp.First, g);
                g.DrawLine(anchorPen, ToPoint(bcp.First), ToPoint(bcp.Second));
            }
            if (bcp.ValidThird)
            {
                FillSquare(bcp.Third, g);
                g.DrawLine(anchorPen, ToPoint(bcp.Second), ToPoint(bcp.Third));
            }
        }
        private static void FillSquare(PointF p, DrawingContext g)
        {
            g.DrawRectangle(Brushes.Black, null, new Rect(p.X - 3, p.Y - 3, 6, 6));
        }
        private static Point ToPoint(PointF p)
        {
            return new Point(p.X, p.Y);
        }
        public void DrawandRefresh()
        {
            DrawAndRefresh();
        }

        private static bool IsInside(PointF target, PointF src)
        {
            if (target.X - 5 <= src.X && src.X <= target.X + 5)
            {
                if (target.Y - 5 <= src.Y && src.Y <= target.Y + 5)
                {
                    return true;
                }
            }
            return false;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            BezierControlPoint p1 = bcps[0];
            BezierControlPoint p2 = bcps[1];
            var pos = e.GetPosition(this);
            var downpos = new PointF((float)pos.X, (float)pos.Y);
            selected = null;
            if (IsInside(p1.Third, downpos))
            {
                selected = p1;
            }
            else if (IsInside(p2.First, downpos))
            {
                selected = p2;
            }
            if (selected != null)
            {
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = e.GetPosition(this);
            float x = (float)pos.X, y = (float)pos.Y;
            if (x < 0) x = 0;
            if (x > ActualWidth) x = (float)ActualWidth;
            if (y < 0) y = 0;
            if (y > ActualHeight) y = (float)ActualHeight;

            var mousepos = new PointF(x, y);
            if (selected != null)
            {
                if (selected == bcps[0])
                {
                    var f = new PointF(mousepos.X - selected.Second.X, mousepos.Y - selected.Second.Y);
                    var nv = BezierCaliculate.GetNormalizePoint(f);
                    selected.ThirdDirection = nv;
                    selected.ThirdLength = GetLength(f);
                    IsLinear = false;
                    DrawandRefresh();
                }
                else if (selected == bcps[1])
                {
                    var f = new PointF(mousepos.X - selected.Second.X, mousepos.Y - selected.Second.Y);
                    var nv = BezierCaliculate.GetNormalizePoint(f);
                    selected.FirstDirection = nv;
                    selected.FirstLength = GetLength(f);
                    IsLinear = false;
                    DrawandRefresh();
                }
            }
        }

        private static float GetLength(PointF p1)
        {
            return (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            selected = null;
        }
    }
}
