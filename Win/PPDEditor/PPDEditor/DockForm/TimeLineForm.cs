using PPDEditor.Controls;
using PPDEditor.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public partial class TimeLineForm : ChangableDockContent
    {
        Timer timer;
        TimeLineRowManager rowManager = new TimeLineRowManager();
        DateTime presetsLoadTime;

        public TimeLineRowManager RowManager
        {
            get
            {
                return rowManager;
            }
        }

        public TimeLineForm()
        {
            InitializeComponent();
            this.SizeChanged += TimeLineForm_SizeChanged;
            seekex1.setseekmain(seekmain1);
            seekex1.MouseDown += seekex1_MouseDown;
            seekmain1.MouseDown += seekmain1_MouseDown;
            seekmain1.KeyDown += seekmain1_KeyDown;
            seekex1.KeyDown += seekex1_KeyDown;
            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += timer_Tick;
        }

        public void SetLang()
        {
            this.選択中のマークにIDを割り当てToolStripMenuItem.Text = Utility.Language["AssignID"];
            this.選択中のマークのID割り当てを解除ToolStripMenuItem.Text = Utility.Language["DeassignID"];
            this.角度を線形補完ToolStripMenuItem.Text = Utility.Language["InterpolateAngle"];
            this.時計回りToolStripMenuItem.Text = Utility.Language["Clockwise"];
            this.反時計回りToolStripMenuItem.Text = Utility.Language["Unclockwise"];
            this.位置を線形補完ToolStripMenuItem.Text = Utility.Language["InterpolatePosition"];
            this.パラメーターToolStripMenuItem.Text = Utility.Language["Parameter"];
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.SuspendLayout();
            GetBackground();
            timer.Stop();
            this.ResumeLayout();
        }

        void seekex1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.DockPanel.ActiveContent == this)
            {
                if (this.DockPanel.FindForm() is EditorForm fm1)
                {
                    fm1.PlayOrPause();
                }
            }
        }

        void seekmain1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.DockPanel.ActiveContent == this)
            {
                if (this.DockPanel.FindForm() is EditorForm fm1)
                {
                    fm1.PlayOrPause();
                }
            }
        }
        public void DxFormPreviewKeyDown(Keys key, bool Control, bool Shift, bool Alt)
        {
            seekmain1.CheckPreviewKey(key, Control, Shift, Alt);
        }
        public void InvalidateAll()
        {
            seekex1.DrawAndRefresh();
            seekmain1.DrawAndRefresh();
        }
        void seekmain1_MouseDown(object sender, MouseEventArgs e)
        {
            this.Pane.Activate();
        }

        void seekex1_MouseDown(object sender, MouseEventArgs e)
        {
            this.Pane.Activate();
        }

        public Seekmain Seekmain
        {
            get
            {
                return this.seekmain1;
            }
        }
        public void SetSkin()
        {
            seekex1.SetSkin();
            Seekmain.SetSkin();
        }

        private void GetBackground()
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

        private void TimeLineForm_DockStateChanged(object sender, EventArgs e)
        {
            if (timer != null) timer.Start();
        }

        private void TimeLineForm_SizeChanged(object sender, EventArgs e)
        {
            if (timer != null) timer.Start();
        }

        private void TimeLineForm_BackgroundImageChanged(object sender, EventArgs e)
        {
            if (this.BackgroundImage != null && seekex1.Width > 0 && seekex1.Height > 0 && seekmain1.Width > 0 && seekmain1.Height > 0)
            {
                var bit = new Bitmap(seekex1.Width, seekex1.Height);
                var g = Graphics.FromImage(bit);
                g.DrawImage(this.BackgroundImage, new Rectangle(0, 0, bit.Width, bit.Height), new Rectangle(0, 0, bit.Width, bit.Height), GraphicsUnit.Pixel);
                seekex1.BackgroundImage = bit;
                seekex1.DrawAndRefresh();
                bit = new Bitmap(seekmain1.Width, seekmain1.Height);
                g = Graphics.FromImage(bit);
                var pos = this.PointToClient(seekmain1.PointToScreen(seekmain1.Location));
                g.DrawImage(this.BackgroundImage, new Rectangle(0, 0, bit.Width, bit.Height), new Rectangle(pos.X, pos.Y, bit.Width, bit.Height), GraphicsUnit.Pixel);
                seekmain1.BackgroundImage = bit;
                seekmain1.DrawAndRefresh();
            }
        }

        private void 選択中のマークにIDを割り当てToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seekmain.AssignID();
        }

        private void 選択中のマークのID割り当てを解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seekmain.UnassignID(() =>
            {
                return MessageBox.Show(Utility.Language["UnassignIDConfirm"], Utility.Language["Confirm"], MessageBoxButtons.OKCancel) == DialogResult.OK;
            });
        }

        private void 時計回りToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seekmain.AngleInterpolation(true);
        }

        private void 反時計回りToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seekmain.AngleInterpolation(false);
        }

        private void 位置を線形補完ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seekmain.PositionInterpolation();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (presetsLoadTime != WindowUtility.InfoForm.PresetsLoadTime)
            {
                パラメーターToolStripMenuItem.DropDownItems.Clear();
                Copy(パラメーターToolStripMenuItem, WindowUtility.InfoForm.PresetsMenuList);
                presetsLoadTime = WindowUtility.InfoForm.PresetsLoadTime;
            }
            Utility.ChangeToolStripCheckState(パラメーターToolStripMenuItem);
        }

        private void Copy(ToolStripMenuItem parent, ToolStripItemCollection collection)
        {
            foreach (ToolStripMenuItem menuItem in collection)
            {
                var commandMenuItem = menuItem as InfoForm.CommandToolStripMenuItem;
                ToolStripMenuItem child;
                if (menuItem != null)
                {
                    child = commandMenuItem.Clone();
                    child.Click += child_Click;
                }
                else
                {
                    child = new ToolStripMenuItem();
                    child.Text = child.ToolTipText = menuItem.Text;
                }
                parent.DropDownItems.Add(child);
            }
        }

        void child_Click(object sender, EventArgs e)
        {
            if (sender is InfoForm.CommandToolStripMenuItem menuItem)
            {
                if (menuItem.CheckState == CheckState.Checked)
                {
                    WindowUtility.InfoForm.UnapplyParameters(menuItem.Preset.Parameters);
                }
                else
                {
                    WindowUtility.InfoForm.ApplyParameters(menuItem.Preset.Parameters);
                }
            }
        }
    }
}
