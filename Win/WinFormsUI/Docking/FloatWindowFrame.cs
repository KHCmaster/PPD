using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public partial class FloatWindowFrame : Form
    {
        const int borderSize = 5;
        private DockPanel dockPanel;
        private FloatWindow innerForm;
        private Bitmap topLeftImage;
        private Bitmap topRightImage;
        private Bitmap bottomLeftImage;
        private Bitmap bottomRightImage;
        private DockPaneCaptionBase caption;
        private AnchorStyles resizeType = AnchorStyles.None;
        private Point resizeStartPosition = Point.Empty;
        private Rectangle resizeStartBound = Rectangle.Empty;

        public FloatWindowFrame(DockPanel dockPanel, FloatWindow innerForm)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);

            this.dockPanel = dockPanel;
            this.innerForm = innerForm;
            CreateEdgeImage();
            InitializeComponent();
            caption = dockPanel.DockPaneCaptionFactory.CreateDockPaneCaption(innerForm.p);
            caption.Location = new Point(borderSize, borderSize);
            caption.Height = 20;
            this.Controls.Add(caption);
            innerForm.SizeChanged += new EventHandler(innerForm_SizeChanged);
            innerForm.LocationChanged += new EventHandler(innerForm_LocationChanged);
            MouseMove += new MouseEventHandler(FloatWindowFrame_MouseMove);
            MouseLeave += new EventHandler(FloatWindowFrame_MouseLeave);
            MouseDown += new MouseEventHandler(FloatWindowFrame_MouseDown);
            MouseUp += new MouseEventHandler(FloatWindowFrame_MouseUp);
            timer1.Tick += new EventHandler(timer1_Tick);
            ChangePosition();
            ChangeSize();
        }

        internal void RefreshChanges()
        {
            if (caption != null)
            {
                caption.RefreshChanges();
            }
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            innerForm.SuspendLayout();
            if (resizeType.HasFlag(AnchorStyles.Right))
            {
                int newWidth = resizeStartBound.Width + Cursor.Position.X - resizeStartPosition.X;
                if (newWidth < 0)
                {
                    newWidth = 0;
                }
                if (innerForm.Width != newWidth)
                {
                    innerForm.Width = newWidth;
                }
            }
            if (resizeType.HasFlag(AnchorStyles.Bottom))
            {
                int newHeight = resizeStartBound.Height + Cursor.Position.Y - resizeStartPosition.Y;
                if (newHeight < 0)
                {
                    newHeight = 0;
                }
                if (innerForm.Height != newHeight)
                {
                    innerForm.Height = newHeight;
                }
            }
            if (resizeType.HasFlag(AnchorStyles.Left))
            {
                int newWidth = resizeStartBound.Width - Cursor.Position.X + resizeStartPosition.X;
                if (newWidth < 0)
                {
                    newWidth = 0;
                }
                if (innerForm.Width != newWidth)
                {
                    innerForm.Width = newWidth;
                    innerForm.Location = new Point(resizeStartBound.X - (newWidth - resizeStartBound.Width), innerForm.Location.Y);
                }
            }
            if (resizeType.HasFlag(AnchorStyles.Top))
            {
                int newHeight = resizeStartBound.Height - Cursor.Position.Y + resizeStartPosition.Y;
                if (newHeight < 0)
                {
                    newHeight = 0;
                }
                if (innerForm.Height != newHeight)
                {
                    innerForm.Height = newHeight;
                    innerForm.Location = new Point(innerForm.Location.X, resizeStartBound.Y - (newHeight - resizeStartBound.Height));
                }
            }
            innerForm.ResumeLayout();
        }

        void FloatWindowFrame_MouseUp(object sender, MouseEventArgs e)
        {
            timer1.Stop();
        }

        void FloatWindowFrame_MouseDown(object sender, MouseEventArgs e)
        {
            resizeType = GetAnchor(e.Location);
            if (resizeType != AnchorStyles.None)
            {
                resizeStartPosition = PointToScreen(e.Location);
                resizeStartBound = innerForm.RectangleToScreen(innerForm.ClientRectangle);
                timer1.Start();
            }
        }

        void FloatWindowFrame_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        void FloatWindowFrame_MouseMove(object sender, MouseEventArgs e)
        {
            AnchorStyles anchor = GetAnchor(e.Location);
            if (anchor == (AnchorStyles.Top | AnchorStyles.Left) || anchor == (AnchorStyles.Bottom | AnchorStyles.Right))
            {
                Cursor = Cursors.SizeNWSE;
            }
            else if (anchor == (AnchorStyles.Top | AnchorStyles.Right) || anchor == (AnchorStyles.Bottom | AnchorStyles.Left))
            {
                Cursor = Cursors.SizeNESW;
            }
            else if (anchor == AnchorStyles.Top || anchor == AnchorStyles.Bottom)
            {
                Cursor = Cursors.SizeNS;
            }
            else if (anchor == AnchorStyles.Left || anchor == AnchorStyles.Right)
            {
                Cursor = Cursors.SizeWE;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private AnchorStyles GetAnchor(Point p)
        {
            if (new Rectangle(0, 0, borderSize, borderSize).Contains(p))
            {
                return AnchorStyles.Top | AnchorStyles.Left;
            }
            else if (new Rectangle(Width - borderSize, Height - borderSize, borderSize, borderSize).Contains(p))
            {
                return AnchorStyles.Bottom | AnchorStyles.Right;
            }
            else if (new Rectangle(Width - borderSize, 0, borderSize, borderSize).Contains(p))
            {
                return AnchorStyles.Top | AnchorStyles.Right;
            }
            else if (new Rectangle(0, Height - borderSize, borderSize, borderSize).Contains(p))
            {
                return AnchorStyles.Bottom | AnchorStyles.Left;
            }
            else if (new Rectangle(borderSize, 0, Width - 2 * borderSize, borderSize).Contains(p))
            {
                return AnchorStyles.Top;
            }
            else if (new Rectangle(borderSize, Height - borderSize, Width - 2 * borderSize, borderSize).Contains(p))
            {
                return AnchorStyles.Bottom;
            }
            else if (new Rectangle(0, borderSize, borderSize, Height - 2 * borderSize).Contains(p))
            {
                return AnchorStyles.Left;
            }
            else if (new Rectangle(Width - borderSize, borderSize, borderSize, Height - 2 * borderSize).Contains(p))
            {
                return AnchorStyles.Right;
            }

            return AnchorStyles.None;
        }

        void innerForm_LocationChanged(object sender, EventArgs e)
        {
            ChangePosition();
            Invalidate();
        }

        void innerForm_SizeChanged(object sender, EventArgs e)
        {
            ChangeSize();
            Invalidate();
        }

        private void ChangePosition()
        {
            this.Location = new Point(innerForm.Location.X - borderSize, innerForm.Location.Y - borderSize - caption.Height);
        }
        private void ChangeSize()
        {
            this.Size = new Size(innerForm.Width + borderSize * 2, innerForm.Height + borderSize * 2 + caption.Height);
            caption.Size = new Size(Width - 2 * borderSize, caption.Height);
        }

        private void CreateEdgeImage()
        {
            topLeftImage = new Bitmap(borderSize, borderSize);
            using (Graphics g = Graphics.FromImage(topLeftImage))
            {
                g.FillRectangle(Brushes.Magenta, new Rectangle(0, 0, borderSize, borderSize));
                using (SolidBrush brush = new SolidBrush(dockPanel.BackColor))
                {
                    g.FillEllipse(brush, new Rectangle(0, 0, borderSize * 2, borderSize * 2));
                }
            }
            topRightImage = new Bitmap(topLeftImage);
            topRightImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bottomLeftImage = new Bitmap(topLeftImage);
            bottomLeftImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bottomRightImage = new Bitmap(topLeftImage);
            bottomRightImage.RotateFlip(RotateFlipType.RotateNoneFlipXY);
        }

        private void FloatWindowFrame_Paint(object sender, PaintEventArgs e)
        {
            Console.Write("paint");
            e.Graphics.Clear(Color.Magenta);
            using (SolidBrush brush = new SolidBrush(dockPanel.BackColor))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(0, borderSize, borderSize, Height - 2 * borderSize));
                e.Graphics.FillRectangle(brush, new Rectangle(borderSize, 0, Width - 2 * borderSize, borderSize));
                e.Graphics.FillRectangle(brush, new Rectangle(Width - borderSize, borderSize, borderSize, Height - 2 * borderSize));
                e.Graphics.FillRectangle(brush, new Rectangle(borderSize, Height - borderSize, Width - 2 * borderSize, borderSize));
                e.Graphics.DrawImage(topLeftImage, new Point(0, 0));
                e.Graphics.DrawImage(topRightImage, new Point(Width - borderSize, 0));
                e.Graphics.DrawImage(bottomLeftImage, new Point(0, Height - borderSize));
                e.Graphics.DrawImage(bottomRightImage, new Point(Width - borderSize, Height - borderSize));
            }
        }
    }
}
