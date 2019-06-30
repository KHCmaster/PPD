using System;
using DigitalRune.Windows.Docking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace testgame
{
    public partial class DXForm : DockableForm
    {
        TimeLineForm tlf;
        public DXForm()
        {
            InitializeComponent();
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(movemark);
            this.AutoScaleDimensions = new SizeF(16f / 9, 1);
        }
        public void settlf(TimeLineForm tlf)
        {
            this.tlf = tlf;
        }
        public void setlang(settinganalyzer sanal)
        {
            this.画面比を169にToolStripMenuItem.Text = sanal.getData("ChangeAspect");
        }
        private void movemark(object sender, EventArgs e)
        {
            Point pos = this.PointToClient(Cursor.Position);
            tlf.seekmain.movemark(pos, this.Width, this.Height);
        }

        private void DXFrom_MouseDown(object sender, MouseEventArgs e)
        {
            timer1.Start();
        }

        private void DXFrom_MouseUp(object sender, MouseEventArgs e)
        {
            timer1.Stop();
            tlf.seekmain.stopmarkedit();
        }

        private void 画面比を169にToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Size size;
            if ((float)this.Width / this.Height > 16f / 9)
            {
                size = new Size((int)(this.Height * 16f / 9), this.Height);
            }
            else
            {
                size = new Size(this.Width, (int)(this.Width * 9f / 16));
            }

            this.FloatAt(new Rectangle(this.PointToScreen(this.Location), new Size(size.Width + 10, size.Height + 30)));

        }

        private void DXFrom_DockStateChanged(object sender, EventArgs e)
        {
            if (this.DockState == DockState.Floating)
            {
                this.画面比を169にToolStripMenuItem.Enabled = true;
            }
            else
            {
                this.画面比を169にToolStripMenuItem.Enabled = false;
            }
        }

        private void DXForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.DockPanel.ActiveContent == this)
            {
                tlf.dxformkeydown(e.KeyCode, true);
            }
            else
            {
                tlf.dxformkeydown(e.KeyCode, false);
            }
        }

        private void DXForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && this.DockPanel.ActiveContent == this)
            {
                e.IsInputKey = true;
            }
            tlf.dxformpreviewkeydown(e.KeyCode);
        }
    }
}
