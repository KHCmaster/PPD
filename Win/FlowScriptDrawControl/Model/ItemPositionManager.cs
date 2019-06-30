using FlowScriptDrawControl.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FlowScriptDrawControl.Model
{
    class ItemPositionManager
    {
        private Point mouseScreenPos;
        private SourceControl[] sources;
        private PositionInfo[] poses;
        private int current = -1;

        public bool HasPositions
        {
            get
            {
                return poses.Length > 0;
            }
        }

        public double CurrentShiftX
        {
            get;
            set;
        }

        public double CurrentShiftY
        {
            get;
            set;
        }

        public ItemPositionManager(Point mouseScreenPos, SourceControl[] sources, double currentShiftX, double currentShiftY)
        {
            this.mouseScreenPos = mouseScreenPos;
            this.sources = sources;
            CurrentShiftX = currentShiftX;
            CurrentShiftY = currentShiftY;
        }

        public ItemPositionManager(Point mouseScreenPos, Point[] points, double currentShiftX, double currentShiftY)
        {
            this.mouseScreenPos = mouseScreenPos;
            var temp = new List<PositionInfo>();
            foreach (var point in points)
            {
                temp.Add(new PositionInfo
                {
                    IsConnected = true,
                    Point = point
                });
            }
            temp.Sort((Comparison<PositionInfo>)((p1, p2) => (int)((p1.Point - mouseScreenPos).Length - (p2.Point - mouseScreenPos).Length)));
            poses = temp.ToArray();
            CurrentShiftX = currentShiftX;
            CurrentShiftY = currentShiftY;
        }

        public void Create()
        {
            if (sources == null)
            {
                return;
            }

            var points = new List<PositionInfo>();
            foreach (var control in sources)
            {
                foreach (var item in control.CurrentSource.InItems.Where(i => i.IsConnectable))
                {
                    var itemControl = control.InItems.FirstOrDefault(ii => ii.CurrentItem == item);
                    if (itemControl == null)
                    {
                        continue;
                    }
                    if (control.CurrentSource.IsCollapsed && item.InConnection == null)
                    {
                        break;
                    }

                    var point = itemControl.PointToScreen(new Point(10, itemControl.ActualHeight / 2));
                    points.Add(new PositionInfo
                    {
                        Point = point,
                        IsConnected = item.InConnection != null
                    });
                }
            }
            points.Sort((Comparison<PositionInfo>)((p1, p2) => (int)((p1.Point - mouseScreenPos).Length - (p2.Point - mouseScreenPos).Length)));
            poses = points.ToArray();
        }

        public Point Next(bool skipConnected)
        {
            var start = current;
            while (true)
            {
                start++;
                if (start >= poses.Length)
                {
                    start = 0;
                }
                if (start == current)
                {
                    break;
                }
                if (!skipConnected)
                {
                    break;
                }
                if (!poses[start].IsConnected)
                {
                    break;
                }
            }
            current = start;
            return poses[start].Point;
        }

        class PositionInfo
        {
            public Point Point { get; set; }
            public bool IsConnected { get; set; }
        }
    }
}
