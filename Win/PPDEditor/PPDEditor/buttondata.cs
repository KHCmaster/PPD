using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;


namespace testgame
{
    public partial class buttondata : UserControl
    {
        SortedList<float, Mark> data;
        Dictionary<string, Picture> resource;
        string[] filenames;
        float[] eval;
        public int start = 0;
        int index = 0;
        const int defaultstart = 95;
        const int defaultinterval = 180;
        const int tamawidth = 10;
        public int bpm = 100;
        public int bpmoffset = 0;
        public int seekpoint = 0;
        Device device;
        Sprite sprite;
        int currentnum = -1;
        public buttondata()
        {
            InitializeComponent();
            data = new SortedList<float, Mark>();
            this.LostFocus += new EventHandler(this.lostfocus);
            this.timer1.Interval = 100;
            this.timer1.Tick += new EventHandler(this.movemark);
        }
        private void movemark(object sender, EventArgs e)
        {
            Point p = this.PointToClient(Cursor.Position);
            if (currentnum != -1)
            {
                Mark mk = null;
                if (data.TryGetValue(data.Keys[currentnum], out mk))
                {
                    data.RemoveAt(currentnum);
                    if (p.X <= defaultstart) p.X = defaultstart;
                    if (p.X >= this.Width) p.X = this.Width;
                    mk.time = (start + p.X - defaultstart) / (float)defaultinterval;
                    while (data.ContainsKey(mk.time))
                    {
                        mk.time -= 0.00001f;
                    }
                    data.Add(mk.time, mk);
                    currentnum = data.IndexOfKey(mk.time);
                    Invalidate();
                }
            }
        }
        private void lostfocus(object sender, EventArgs e)
        {
            currentnum = -1;
            Invalidate();
        }
        public void setinfo(string name,int index)
        {
            System.Reflection.Assembly asm;
            asm = System.Reflection.Assembly.GetExecutingAssembly();
            string s = asm.GetName().Name;
            System.Resources.ResourceManager rm = global::testgame.Properties.Resources.ResourceManager;
            pictureBox1.Image = (Bitmap)rm.GetObject(name);
            this.index = index;
        }
        public void setresource(Dictionary<string, Picture> resource, string[] filenames,Device device,Sprite sprite,float[] eval)
        {
            this.resource = resource;
            this.filenames = filenames;
            this.device = device;
            this.sprite = sprite;
            this.eval = eval;
        }
        public void adddata(float time)
        {
            if (!data.ContainsKey(time))
            {
                Mark mk = new Mark(device, sprite, resource, filenames, index, 400, 225, time, 0, eval);
                data.Add(time, mk);
                currentnum = data.IndexOfKey(time);
                Invalidate();
            }
        }
        private void buttondata_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);
            pictureBox1.BackColor = Color.White;
            float bpminterval = (60f / bpm * defaultinterval);
            int asda = (int)((start) / bpminterval);
            for (int i = asda; (i + (float)bpmoffset / bpm) * bpminterval - start + defaultstart <= this.Width; i++)
            {
                g.DrawLine(Pens.Blue, (i + (float)bpmoffset / bpm) * bpminterval - start + defaultstart, 0, (i + (float)bpmoffset / bpm) * bpminterval - start + defaultstart, this.Height - 1);
            }
            float st = start / (float)defaultinterval;
            float end = start / (float)defaultinterval + (this.Width - defaultstart) / (float)defaultinterval;
            foreach (float t in data.Keys)
            {
                if (t < st) continue;
                if (t > end) break;
                g.FillEllipse(data.IndexOfKey(t) == currentnum?Brushes.Blue:Brushes.Black, defaultstart + (int)(t * defaultinterval) - start-tamawidth/2, 7, tamawidth, tamawidth);
            }
            g.DrawLine(Pens.Gray, defaultstart, 0, defaultstart, this.Height);
            g.FillRectangle(Brushes.White, 0, 0, defaultstart, this.Height);
            g.DrawLine(Pens.Red, defaultstart + seekpoint, 0, defaultstart + seekpoint, this.Height);
            g.DrawLine(Pens.Gray, 0, this.Height - 1, this.Width, this.Height - 1);
            g.Dispose();
        }

        private void buttondata_SizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }



        private bool finddata(Point p)
        {
            float st = start / (float)defaultinterval;
            float end = start / (float)defaultinterval + (this.Width - defaultstart) / (float)defaultinterval;
            foreach (float t in data.Keys)
            {
                if (t < st) continue;
                if (t > end) break;
                int px = defaultstart + (int)(t * defaultinterval) - start - tamawidth / 2;
                if (px <= p.X && p.X <= px + tamawidth)
                {
                    currentnum = data.IndexOfKey(t);
                    return true;
                }
            }
            return false;
        }

        private void buttondata_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.X >= defaultstart && e.X <= this.Width)
            {
                if (!finddata(e.Location))
                {
                    adddata((start + e.X - defaultstart) / (float)defaultinterval);
                }
                else
                {
                    this.timer1.Start();
                }
            }
        }

        private void buttondata_MouseUp(object sender, MouseEventArgs e)
        {
            this.timer1.Stop();
        }

        private void buttondata_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && currentnum != -1)
            {
                data.RemoveAt(currentnum);
                currentnum = -1;
                Invalidate();
            }
                
        }
    }
}
