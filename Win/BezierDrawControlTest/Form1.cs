using BezierCaliculator;
using BezierDrawControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BezierDrawControlTest
{
    public partial class Form1 : Form
    {
        PointF[] poses = new PointF[10];
        PointF[] dirs = new PointF[10];

        SquareGrid grid = new SquareGrid
        {
            Width = 50,
            Height = 50,
            OffsetX = 200,
            OffsetY = 200
        };

        public Form1()
        {
            InitializeComponent();
            // bezierControl1.AntialiasEnabled = true;
            bezierControl1.Controller.AfterPaint += bezierControl1_UserPaint;
            bezierControl1.Controller.PreprocessPoint += bezierControl1_PreprocessPoint;
            bezierControl1.Controller.RestrictAngleSplit = 3;
            //CreateCircle();
        }

        private void CreateCircle()
        {
            float temp = 0.5522847f;
            var data = new List<BezierControlPoint>();
            float radius = 100;
            var bcp = new BezierControlPoint
            {
                Second = new PointF(radius, 0),
                First = new PointF(radius, -radius * temp),
                Third = new PointF(radius, radius * temp),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            bcp = new BezierControlPoint
            {
                Second = new PointF(0, radius),
                First = new PointF(radius * temp, radius),
                Third = new PointF(-radius * temp, radius),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            bcp = new BezierControlPoint
            {
                Second = new PointF(-radius, 0),
                First = new PointF(-radius, radius * temp),
                Third = new PointF(-radius, -radius * temp),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            bcp = new BezierControlPoint
            {
                Second = new PointF(0, -radius),
                First = new PointF(-radius * temp, -radius),
                Third = new PointF(radius * temp, -radius),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            data.Add(data[0]);
            bezierControl1.Controller.BCPS = data.ToArray();
        }

        void bezierControl1_UserPaint(object sender, UserPaintEventArgs e)
        {
            if (bezierControl1.Controller.BCPSCount >= 2)
            {
                BezierControlPoint[] bcps = bezierControl1.Controller.BCPS;
                var ba = new BezierAnalyzer(bcps);
                for (int i = 0; i < poses.Length; i++)
                {
                    ba.GetPoint(i * 99.9999f / 9, out PointF pos, out PointF dir);
                    poses[i] = pos;
                    dirs[i] = dir;
                }
                ba.GetYFromX(this.Width / 2);
            }
#if DRAW
                foreach (PointF pos in poses)
                {
                    e.Graphic.DrawEllipse(Pens.Black, pos.X - 5, pos.Y - 5, 10, 10);
                }
                for (int i = 0; i < dirs.Length; i++)
                {
                    DrawAllow(poses[i], dirs[i], e.Graphic);
                }
            }
#endif
            var graphics = ((BezierControl.Context)e.Context).Graphics;
            for (int i = -grid.OffsetX / grid.Width; i <= bezierControl1.Width / grid.Width; i++)
            {
                graphics.DrawLine(Pens.Black, new Point(i * grid.Width + grid.OffsetX, 0), new Point(i * grid.Width + grid.OffsetX, bezierControl1.Height));
            }
            for (int i = -grid.OffsetY / grid.Height; i <= bezierControl1.Height / grid.Height; i++)
            {
                graphics.DrawLine(Pens.Black, new Point(0, i * grid.Height + grid.OffsetY), new Point(bezierControl1.Width, i * grid.Height + grid.OffsetY));
            }
        }

        PointF bezierControl1_PreprocessPoint(PointF arg)
        {
            int offsetx = grid.NormalizedOffsetX;
            int offsety = grid.NormalizedOffsetY;
            var nearest = new PointF((int)((arg.X + grid.Width / 2)) / grid.Width * grid.Width + offsetx,
                (int)((arg.Y + grid.Height / 2)) / grid.Height * grid.Height + offsety);
            var diff = new PointF(nearest.X - arg.X, nearest.Y - arg.Y);

            if (Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) >= 6)
            {
                return arg;
            }

            return nearest;
        }

        private void DrawAllow(PointF pos, PointF dir, Graphics g)
        {
            float length = 100;
            g.DrawLine(Pens.Black, pos, new PointF(pos.X + dir.X * length, pos.Y + dir.Y * length));
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z:
                        bezierControl1.Controller.Undo();
                        break;
                    case Keys.Y:
                        bezierControl1.Controller.Redo();
                        break;
                    case Keys.T:
                        bezierControl1.Controller.StartTransform();
                        break;
                }
            }
            else
            {
                if (e.KeyCode == Keys.Delete)
                {
                    bezierControl1.Controller.Delete();
                }
                if (e.KeyCode == Keys.Return)
                {
                    bezierControl1.Controller.EndTransform();
                }
            }
        }
    }
}
