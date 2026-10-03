using BezierCaliculator;
using BezierDrawControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace BezierDrawControlTest
{
    public partial class MainWindow : Window
    {
        const int CanvasWidth = 800;
        const int CanvasHeight = 450;

        readonly PointF[] poses = new PointF[10];
        readonly PointF[] dirs = new PointF[10];

        readonly SquareGrid grid = new()
        {
            Width = 50,
            Height = 50,
            OffsetX = 200,
            OffsetY = 200
        };

        public MainWindow()
        {
            InitializeComponent();
            bezierControl1.Initialize(CanvasWidth, CanvasHeight);
            bezierControl1.Controller.AfterPaint += BezierControl1_UserPaint;
            bezierControl1.Controller.PreprocessPoint += BezierControl1_PreprocessPoint;
            bezierControl1.Controller.RestrictAngleSplit = 3;
            //CreateCircle();
            bezierControl1.DrawAndRefresh();
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
            bezierControl1.Controller.BCPS = [.. data];
        }

        void BezierControl1_UserPaint(object sender, UserPaintEventArgs e)
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
                ba.GetYFromX(CanvasWidth / 2);
            }
            for (int i = -grid.OffsetX / grid.Width; i <= CanvasWidth / grid.Width; i++)
            {
                e.Context.DrawLine(Color.Black, new PointF(i * grid.Width + grid.OffsetX, 0), new PointF(i * grid.Width + grid.OffsetX, CanvasHeight));
            }
            for (int i = -grid.OffsetY / grid.Height; i <= CanvasHeight / grid.Height; i++)
            {
                e.Context.DrawLine(Color.Black, new PointF(0, i * grid.Height + grid.OffsetY), new PointF(CanvasWidth, i * grid.Height + grid.OffsetY));
            }
        }

        PointF BezierControl1_PreprocessPoint(PointF arg)
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.Z:
                        bezierControl1.Controller.Undo();
                        break;
                    case Key.Y:
                        bezierControl1.Controller.Redo();
                        break;
                    case Key.T:
                        bezierControl1.Controller.StartTransform();
                        break;
                }
            }
            else
            {
                if (e.Key == Key.Delete)
                {
                    bezierControl1.Controller.Delete();
                }
                if (e.Key == Key.Return)
                {
                    bezierControl1.Controller.EndTransform();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bezierControl1.Dispose();
        }
    }
}
