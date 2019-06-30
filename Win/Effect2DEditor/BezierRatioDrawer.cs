using BezierCaliculator;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class BezierRatioDrawer : UserControl
    {
        private BufferedGraphics grafx;
        private BufferedGraphicsContext context;
        private BezierControlPoint selected;
        private BezierControlPoint[] bcps;
        public BezierRatioDrawer()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
            SetLinearRatio();
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
            bcps = new BezierControlPoint[]{
                BezierControlPoint.Deserialize("VT=1 SX=0 SY=128 TDX=0.707106781 TDY=-0.707106781 TL=60"),
                BezierControlPoint.Deserialize("VF=1 SX=128 SY=0 FDX=-0.707106781 FDY=0.707106781 FL=60")
            };
            IsLinear = true;
            DrawandRefresh();
        }
        public bool IsLinear
        {
            get;
            private set;
        }
        private void DrawToBuffer(Graphics g)
        {
            g.Clear(Color.White);
            if (bcps.Length == 2)
            {
                DrawNormalData(g);
            }
        }
        private void DrawNormalData(Graphics g)
        {
            BezierControlPoint p1 = bcps[0];
            BezierControlPoint p2 = bcps[1];
            DrawBezeier(p1.Second, p1.Third, p2.First, p2.Second, g);
            DrawAnchor(p1, g);
            DrawAnchor(p2, g);
        }
        private void DrawBezeier(PointF p1, PointF p2, PointF p3, PointF p4, Graphics g)
        {
            PointF lastbezeirpoint, bezeirpoint;
            lastbezeirpoint = p1;
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                bezeirpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                g.DrawLine(Pens.Black, lastbezeirpoint, bezeirpoint);
                lastbezeirpoint = bezeirpoint;
            }
        }
        private void DrawAnchor(BezierControlPoint bcp, Graphics g)
        {
            var pen = new Pen(Color.Blue, 2);
            if (bcp.ValidFirst)
            {
                FillSquare(bcp.First, g);
                g.DrawLine(pen, bcp.First, bcp.Second);
            }
            if (bcp.ValidThird)
            {
                FillSquare(bcp.Third, g);
                g.DrawLine(pen, bcp.Second, bcp.Third);
            }
            pen.Dispose();
        }
        private void FillSquare(PointF p, Graphics g)
        {
            g.FillRectangle(Brushes.Black, p.X - 3, p.Y - 3, 6, 6);
        }
        private void EffectDrawer_Paint(object sender, PaintEventArgs e)
        {
            grafx.Render(e.Graphics);
        }
        public void DrawandRefresh()
        {
            DrawToBuffer(grafx.Graphics);
            Refresh();
        }

        private bool IsInside(PointF target, PointF src)
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
        private void BezierRatioDrawer_MouseDown(object sender, MouseEventArgs e)
        {
            BezierControlPoint p1 = bcps[0];
            BezierControlPoint p2 = bcps[1];
            var downpos = new PointF(e.X, e.Y);
            selected = null;
            if (IsInside(p1.Third, downpos))
            {
                selected = p1;
            }
            else if (IsInside(p2.First, downpos))
            {
                selected = p2;
            }
        }

        private void BezierRatioDrawer_MouseMove(object sender, MouseEventArgs e)
        {
            float x = e.X, y = e.Y;
            if (x < 0) x = 0;
            if (x > this.Width) x = this.Width;
            if (y < 0) y = 0;
            if (y > this.Height) y = this.Height;

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

        private float GetLength(PointF p1)
        {
            return (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
        }
        private void BezierRatioDrawer_MouseUp(object sender, MouseEventArgs e)
        {
            selected = null;
        }
    }
}
