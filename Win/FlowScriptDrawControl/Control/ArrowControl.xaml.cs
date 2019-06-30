using FlowScriptDrawControl.Model;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// ArrowControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ArrowControl : UserControl
    {
        private Arrow arrow;

        private const int ControlDiff = 30;
        private const int spaceY = 5;
        private const int thickness = 3;

        SourceItemControl srcItem;
        SourceItemControl destItem;

        public SourceItemControl SrcItem
        {
            get { return srcItem; }
            set
            {
                if (srcItem != value)
                {
                    if (srcItem != null)
                    {
                        srcItem.CurrentItem.Source.PropertyChanged -= CurrentPositionable_PropertyChanged;
                        srcItem.CurrentItem.Source.SizeChanged -= CurrentPositionable_SizeChanged;
                    }
                    srcItem = value;
                    arrow.SrcItem = srcItem?.CurrentItem;
                    if (srcItem != null)
                    {
                        srcItem.CurrentItem.Source.PropertyChanged += CurrentPositionable_PropertyChanged;
                        srcItem.CurrentItem.Source.SizeChanged += CurrentPositionable_SizeChanged;
                    }
                }
            }
        }

        public SourceItemControl DestItem
        {
            get { return destItem; }
            set
            {
                if (destItem != value)
                {
                    if (destItem != null)
                    {
                        destItem.CurrentItem.Source.PropertyChanged -= CurrentPositionable_PropertyChanged;
                        destItem.CurrentItem.Source.SizeChanged -= CurrentPositionable_SizeChanged;
                    }
                    destItem = value;
                    arrow.DestItem = destItem?.CurrentItem;
                    if (destItem != null)
                    {
                        destItem.CurrentItem.Source.PropertyChanged += CurrentPositionable_PropertyChanged;
                        destItem.CurrentItem.Source.SizeChanged += CurrentPositionable_SizeChanged;
                    }
                }
            }
        }

        public ArrowControl()
        {
            arrow = new Arrow();
            DataContext = arrow;
            InitializeComponent();

            Loaded += ArrowControl_Loaded;
        }

        public void UpdatePosition(Point from, Point to)
        {
            var mid = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2);
            var diff = Math.Min(100, Math.Max(1, Math.Abs(from.X - to.X) * ControlDiff / 100f));
            var geo = new PathGeometry(new PathFigure[]{
                new PathFigure(from, new QuadraticBezierSegment[]{
                    new QuadraticBezierSegment(new Point(from.X + diff,from.Y), mid, true),                    new QuadraticBezierSegment(new Point(to.X-diff,to.Y),new Point(to.X-thickness,to.Y),true)                },false),                new PathFigure(new Point(to.X-spaceY-thickness,to.Y-spaceY), new LineSegment[]{
                    new LineSegment(new Point(to.X-thickness,to.Y),true),                    new LineSegment(new Point(to.X-spaceY-thickness,to.Y+spaceY),true)                },false)            });
            arrow.Geometry = geo;
        }

        public void UpdatePositionInItem()
        {
            var drawGrid = Utility.GetParent<Grid>(this);
            if (drawGrid == null | Utility.IsLoading)
            {
                return;
            }

            if (srcItem != null && destItem != null)
            {
                var from = drawGrid.PointFromScreen(srcItem.PointToScreen(new Point(srcItem.ActualWidth, srcItem.ActualHeight / 2)));
                var to = drawGrid.PointFromScreen(destItem.PointToScreen(new Point(0, destItem.ActualHeight / 2)));
                UpdatePosition(from, to);
            }
        }

        void CurrentPositionable_SizeChanged()
        {
            UpdatePositionInItem();
        }

        void CurrentPositionable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Transform2")
            {
                UpdatePositionInItem();
            }
        }

        void ArrowControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePositionInItem();
        }
    }
}
