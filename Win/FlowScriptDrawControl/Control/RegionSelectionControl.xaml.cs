using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// RegionSelectionControl.xaml の相互作用ロジック
    /// </summary>
    public partial class RegionSelectionControl : UserControl
    {
        public RegionSelectionControl()
        {
            InitializeComponent();
        }

        public Point TopLeft
        {
            get;
            private set;
        }

        public Point BottomRight
        {
            get;
            private set;
        }

        public void UpdatePosition(Point topLeft, Point bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;

            mainGrid.Children.Clear();
            var path = new Path
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0x44, 0xBB, 0xFF)),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(0x33, 0x44, 0xBB, 0xFF))
            };
            var geo = new PathGeometry(new PathFigure[]{
                new PathFigure(topLeft, new LineSegment[]{
                    new LineSegment(new Point(bottomRight.X, topLeft.Y), true),                    new LineSegment(new Point(bottomRight.X, bottomRight.Y), true),                    new LineSegment(new Point(topLeft.X, bottomRight.Y), true)                },true)            });
            path.Data = geo;
            mainGrid.Children.Add(path);
        }
    }
}
