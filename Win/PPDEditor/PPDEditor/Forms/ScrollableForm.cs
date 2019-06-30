using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.Forms
{
    public class ScrollableForm : ChangableDockContent
    {
        Timer timer;
        public ScrollableForm()
        {
            InitializeComponent();
            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += timer_Tick;
            AutoScroll = true;
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            // Returning the current location prevents the panel from
            // scrolling to the active control when the panel loses and regains focus
            return this.DisplayRectangle.Location;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.SuspendLayout();
            GetBackGround();
            timer.Stop();
            this.ResumeLayout();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ScrollableForm2
            // 
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Name = "ScrollableForm2";
            this.DockStateChanged += this.ScrollableForm_DockStateChanged;
            this.ResumeLayout(false);

        }
        private void ScrollableForm_DockStateChanged(object sender, EventArgs e)
        {
            timer.Start();
        }
        private void GetBackGround()
        {
            if (DockPanel != null && DockPanel.BackgroundImage != null && Pane != null && Pane.Size.Width > 0 && Pane.Size.Height > 0)
            {
                if (DockState == DockState.Float)
                {
                    var bit = new Bitmap(1000, 1000);
                    var g = Graphics.FromImage(bit);
                    var sb = new SolidBrush(SystemColors.Control);
                    g.FillRectangle(sb, new Rectangle(0, 0, bit.Width, bit.Height));
                    this.BackgroundImage = bit;
                    sb.Dispose();
                }
                else
                {
                    //Console.WriteLine(GetPointFromDockPanel(Pane));
                    var p = GetPointFromDockPanel(Pane);
                    switch (DockState)
                    {
                        case DockState.DockRight:
                            p = new Point(p.X, p.Y + 18);
                            break;
                        case DockState.DockLeft:
                            p = new Point(p.X, p.Y + 18);
                            break;
                        case DockState.DockTop:
                            p = new Point(p.X, p.Y + 18);
                            break;
                        case DockState.DockBottom:
                            p = new Point(p.X, p.Y + 18);
                            break;
                        case DockState.Document:
                            p = new Point(p.X, p.Y + 24);
                            break;
                    }

                    var bit = new Bitmap(Pane.Size.Width, Pane.Size.Height);
                    var g = Graphics.FromImage(bit);

                    g.DrawImage(DockPanel.BackgroundImage, new Rectangle(0, 0, Pane.Size.Width, Pane.Size.Height), new Rectangle(p.X, p.Y, Pane.Size.Width, Pane.Size.Height), GraphicsUnit.Pixel);
                    var sb = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
                    g.FillRectangle(sb, new Rectangle(0, 0, Pane.Size.Width, Pane.Size.Height));
                    sb.Dispose();
                    this.BackgroundImage = bit;
                }
            }
        }
        private Point GetPointFromDockPanel(DockPane pane)
        {
            var ret = new Point(0, 0);
            Control c = pane;
            while (c != null && !(c is DockPanel))
            {
                ret = new Point(ret.X + c.Location.X, ret.Y + c.Location.Y);
                c = c.Parent;
            }
            return ret;
        }
        public void ShowOrHideWindow(DockContent dockcontent)
        {
            if (DockPanel == null)
            {
                dockcontent.Show(WindowUtility.DockPanel, WeifenLuo.WinFormsUI.Docking.DockState.Float);
            }
            else
            {
                if (!dockcontent.Visible || (DockState.DockTopAutoHide <= dockcontent.DockState && dockcontent.DockState <= DockState.DockRightAutoHide))
                {
                    if (dockcontent.Pane != null)
                    {
                        dockcontent.Show(DockPanel);
                        dockcontent.Pane.ActiveContent = dockcontent;
                    }
                    else
                    {
                        dockcontent.Show(DockPanel, WeifenLuo.WinFormsUI.Docking.DockState.Float);
                    }
                }
                else
                {
                    if (DockPanel.ActivePane == dockcontent.Pane || dockcontent.DockState == DockState.Float)
                    {
                    }
                    else
                    {
                        dockcontent.Focus();
                    }
                }
            }
        }
    }
}
