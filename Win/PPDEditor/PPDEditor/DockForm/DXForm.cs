using PPDEditor.Forms;
using SharpDX;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public partial class DXForm : DockContent
    {
        public DXForm()
        {
            InitializeComponent();
            timer1.Interval = 100;
            timer1.Tick += movemark;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16f / 9, 1);
            ControlAdded += DXForm_ControlAdded;
        }

        private void DXForm_ControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.MouseDown += DXForm_MouseDown;
            e.Control.MouseLeave += DXForm_MouseLeave;
            e.Control.MouseUp += DXForm_MouseUp;
            e.Control.MouseMove += DXForm_MouseMove;
            e.Control.KeyDown += DXForm_KeyDown;
            e.Control.PreviewKeyDown += DXForm_PreviewKeyDown;
        }

        public void SetLang()
        {
            this.TabText = this.Text = Utility.Language["GameWindow"];
            this.画面比を169にToolStripMenuItem.Text = Utility.Language["ChangeAspect"];
            this.標準解像度にするToolStripMenuItem.Text = Utility.Language["GWDefaultResolution"];
        }
        private void movemark(object sender, EventArgs e)
        {
            var p = this.PointToClient(Cursor.Position);
            var pos = new Point(p.X, p.Y);
            WindowUtility.Seekmain.MoveMark(pos, this.Width, this.Height);
        }
        private void DXForm_MouseMove(object sender, MouseEventArgs e)
        {
            var p = this.PointToClient(Cursor.Position);
            var pos = new Point(p.X, p.Y);
            WindowUtility.Seekmain.OnMouseMove(pos, this.Width, this.Height);
            //Point truepos = new Point(e.X * 800 / this.Width, e.Y * 450 / this.Height);
            //WindowUtility.Form1.ChangeMousepos(truepos);
        }

        private void DXForm_MouseLeave(object sender, EventArgs e)
        {
            WindowUtility.MainForm.ChangeMousePos(new Point(-1, -1));
        }
        private void DXForm_MouseDown(object sender, MouseEventArgs e)
        {
            timer1.Start();
        }

        private void DXForm_MouseUp(object sender, MouseEventArgs e)
        {
            timer1.Stop();
            WindowUtility.Seekmain.FinishMarkEdit();
        }

        private void 画面比を169にToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Drawing.Size size;
            if ((float)this.Width / this.Height > 16f / 9)
            {
                size = new System.Drawing.Size((int)(this.Height * 16f / 9), this.Height);
            }
            else
            {
                size = new System.Drawing.Size(this.Width, (int)(this.Width * 9f / 16));
            }
            ChangeWindowSize(size);
        }

        private void ChangeWindowSize(System.Drawing.Size size)
        {
            var location = this.PointToScreen(this.Location);
            this.FloatAt(new System.Drawing.Rectangle(location, new System.Drawing.Size(size.Width, size.Height)));
            int diffX = size.Width - ClientSize.Width;
            int diffY = size.Height - ClientSize.Height;
            var locationDiffX = location.X - this.PointToScreen(this.Location).X;
            var locationDiffY = location.Y - this.PointToScreen(this.Location).Y;
            this.FloatAt(new System.Drawing.Rectangle(
                new System.Drawing.Point(location.X + locationDiffX, location.Y + locationDiffY),
                new System.Drawing.Size(size.Width + diffX, size.Height + diffY)));
        }

        private void DXForm_DockStateChanged(object sender, EventArgs e)
        {
            if (this.DockState == DockState.Float)
            {
                this.画面比を169にToolStripMenuItem.Enabled = true;
                this.標準解像度にするToolStripMenuItem.Enabled = true;
            }
            else
            {
                this.画面比を169にToolStripMenuItem.Enabled = false;
                this.標準解像度にするToolStripMenuItem.Enabled = false;
            }
        }

        private void 標準解像度にするToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeWindowSize(new System.Drawing.Size(800, 450));
        }

        private void DXForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.DockPanel.ActiveContent == this)
            {
                if (this.DockPanel.FindForm() is EditorForm fm1)
                {
                    fm1.PlayOrPause();
                }
            }
        }

        private void DXForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.IsInputKey = true;
            }
            WindowUtility.TimeLineForm.DxFormPreviewKeyDown(e.KeyCode, e.Control, e.Shift, e.Alt);
        }
    }
}
