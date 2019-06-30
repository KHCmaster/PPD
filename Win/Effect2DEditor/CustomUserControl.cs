using System;
using System.Drawing;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public class CustomUserControl : UserControl
    {
        private BufferedGraphics grafx;
        private BufferedGraphicsContext context;
        protected void InitializeBuffer()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
            this.SizeChanged += CustomUserControl_SizeChanged;
            this.Paint += CustomUserControl_Paint;
        }

        void CustomUserControl_Paint(object sender, PaintEventArgs e)
        {
            grafx.Render(e.Graphics);
        }

        void CustomUserControl_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width > 0 && this.Height > 0)
            {
                context = BufferedGraphicsManager.Current;
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
                DrawAndRefresh();
            }
        }
        protected virtual void DrawToBuffer(Graphics g)
        {
            g.Clear(this.BackColor == Color.Transparent ? Color.White : this.BackColor);
        }
        public void DrawAndRefresh()
        {
            DrawToBuffer(grafx.Graphics);
            Refresh();
        }
    }
}
