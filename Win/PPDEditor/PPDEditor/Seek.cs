using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace testgame
{
    public partial class Seek : UserControl
    {
        public int start = 0;
        public int defaultstart = 95;
        public int defaultinterval = 180;
        public int seekpoint = 0;
        public int bpm = 100;
        public int bpmoffset = 0;
        buttondata[] buttondatas;
        public Seek()
        {
            InitializeComponent();
            this.timer1.Interval = 50;
            this.timer1.Tick += new EventHandler(moveseek);
        }
        public void setbuttondata(buttondata[] bd)
        {
            buttondatas = bd;
        }
        private void Seek_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.FillRectangle(Brushes.LightGray, 0, 0, this.Width, this.Height);
            g.DrawLine(Pens.Gray, 0, this.Height - 1, this.Width, this.Height - 1);
            g.DrawRectangle(Pens.Red, defaultstart + seekpoint - 5, 0, 10, this.Height - 1);
            float bpminterval = (60f / bpm * defaultinterval);
            int asda = (int)((start) / bpminterval);
            for (int i = asda; (i + (float)bpmoffset / bpm) * bpminterval - start + defaultstart <= this.Width; i++)
            {
                g.DrawLine(Pens.Blue, (i + (float)bpmoffset / bpm) * bpminterval - start + defaultstart, 0, (i + (float)bpmoffset / bpm) * bpminterval - start + defaultstart, this.Height - 1);
            }
            Font font = new Font("ＭＳ ゴシック", 10);
            int shoulddisplay = (start - start % defaultinterval) / defaultinterval;
            for (int i = 0; i * 60 < this.Width; i++)
            {
                g.DrawString((i + shoulddisplay).ToString(), font, Brushes.Gray, defaultstart + defaultinterval * i - start % defaultinterval, 10);
            }
            g.DrawLine(Pens.Gray, 0, this.Height - 1, defaultstart, this.Height - 1);
            g.FillRectangle(Brushes.LightGray, 0, 0, defaultstart, this.Height - 1);
            g.DrawLine(Pens.Gray, defaultstart, 0, defaultstart, this.Height - 1);
            font = new Font("ＭＳ ゴシック", 14);
            g.DrawString("Time Line", font, Brushes.Black, 0, 0);
            g.Dispose();
        }

        private void Seek_MouseDown(object sender, MouseEventArgs e)
        {
            this.timer1.Start();
            if (e.X < defaultstart)
            {
                seekpoint = 0;
            }
            else
            {
                seekpoint = e.X - defaultstart;
            }
            changechildseekandstartpoint();
            Invalidate();
        }
        public void changechildseekandstartpoint()
        {
            if (buttondatas != null)
            {
                for (int i = 0; i < buttondatas.Length; i++)
                {
                    buttondatas[i].seekpoint = this.seekpoint;
                    buttondatas[i].start = start;
                    buttondatas[i].Invalidate();
                }
            }
        }
        private void Seek_MouseUp(object sender, MouseEventArgs e)
        {
            this.timer1.Stop();
        }

        private void moveseek(object sender, EventArgs e)
        {
            Point p = PointToClient(Cursor.Position);
            if (p.X >= this.Width)
            {
                seekpoint = this.Width - defaultstart;
            }
            else if (p.X <= defaultstart)
            {
                seekpoint = 0;
            }
            else
            {
                seekpoint = p.X - defaultstart;
            }
            if (p.X > this.Width)
            {
                start += p.X - this.Width;
            }
            if (p.X < defaultstart)
            {
                start += p.X - defaultstart;
                if (start <= 0)
                {
                    start = 0;
                }
            }
            changechildseekandstartpoint();
            Form1 fm1 = this.Parent.Parent.Parent.Parent.Parent as Form1;
            if (fm1 != null)
            {
                fm1.seekmovie((start + seekpoint - defaultstart) / (double)defaultinterval);
            }
            Invalidate();
        }
    }
}
