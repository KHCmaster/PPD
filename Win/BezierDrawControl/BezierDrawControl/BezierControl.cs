using System;
using System.Drawing;
using System.Windows.Forms;

namespace BezierDrawControl
{
    public partial class BezierControl : UserControl
    {
        public class Context : IBezierDrawContext
        {
            public Graphics Graphics
            {
                get;
                private set;
            }

            public Context(Graphics g)
            {
                Graphics = g;
            }

            public void DrawString(string text, Color color, float fontSize, PointF point)
            {
                using (var font = new Font(DefaultFont.FontFamily, fontSize))
                using (var brush = new SolidBrush(color))
                {
                    Graphics.DrawString(text, font, brush, point);
                }
            }

            public void DrawLines(Color color, PointF[] points)
            {
                using (var pen = new Pen(color))
                {
                    Graphics.DrawLines(pen, points);
                }
            }

            public void DrawLine(Color color, PointF p1, PointF p2)
            {
                using (var pen = new Pen(color))
                {
                    Graphics.DrawLine(pen, p1, p2);
                }
            }

            public void DrawEllipse(Color color, RectangleF rect)
            {
                using (var pen = new Pen(color))
                {
                    Graphics.DrawEllipse(pen, rect);
                }
            }

            public void DrawRectangle(Color color, float x, float y, float width, float height)
            {
                using (var pen = new Pen(color))
                {
                    Graphics.DrawRectangle(pen, x, y, width, height);
                }
            }

            public void FillRectangle(Color color, float x, float y, float width, float height)
            {
                using (var brush = new SolidBrush(color))
                {
                    Graphics.FillRectangle(brush, x, y, width, height);
                }
            }
        }

        private BufferedGraphics grafx;
        private BufferedGraphicsContext context;

        public bool AntialiasEnabled
        {
            get;
            set;
        }

        public BezierController Controller
        {
            get;
            private set;
        }

        public BezierControl()
        {
            Controller = new BezierController();
            Controller.DrawRequired += Controller_DrawRequired;
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
            DrawToBuffer(grafx.Graphics);
        }

        private void Controller_DrawRequired()
        {
            DrawAndRefresh();
        }

        private void BezierControl_Paint(object sender, PaintEventArgs e)
        {
            grafx.Render(e.Graphics);
        }

        private void DrawToBuffer(Graphics g)
        {
            if (AntialiasEnabled)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
            else
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }
            g.Clear(Color.White);
            Controller.Draw(new Context(g));
        }

        public void DrawAndRefresh()
        {
            DrawToBuffer(grafx.Graphics);
            Refresh();
        }

        private void BezierControl_MouseDown(object sender, MouseEventArgs e)
        {
            Controller.MouseDown(e.X, e.Y, e.Button, ModifierKeys);
            DrawAndRefresh();
        }

        private void BezierControl_MouseUp(object sender, MouseEventArgs e)
        {
            Controller.MouseUp();
            DrawAndRefresh();
        }

        private void BezierControl_MouseMove(object sender, MouseEventArgs e)
        {
            var cursor = Controller.MouseMove(e.X, e.Y, ModifierKeys);
            if (cursor != null)
            {
                Cursor = cursor;
            }
        }

        private void BezierControl_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width > 0 && this.Height > 0)
            {
                context = BufferedGraphicsManager.Current;
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
                DrawAndRefresh();
            }
        }
    }
}