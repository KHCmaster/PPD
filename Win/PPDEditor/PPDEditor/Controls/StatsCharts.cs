using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PPDEditor.Controls
{
    /// <summary>
    /// 円グラフの 1 項目です。
    /// </summary>
    public record PieChartItem(string Label, int Value, Color Color);

    /// <summary>
    /// 統計で使う円グラフです。左に円、右に凡例（名前と割合）を書く。
    /// </summary>
    public class PieChart : FrameworkElement
    {
        const double Padding = 10;
        const double LegendBoxSize = 10;
        const double LegendLineHeight = 18;

        IReadOnlyList<PieChartItem> items = [];

        public IReadOnlyList<PieChartItem> Items
        {
            get { return items; }
            set
            {
                items = value ?? [];
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            var total = items.Sum(i => i.Value);
            if (total <= 0)
            {
                return;
            }

            var typeface = ChartText.Typeface;
            var labels = items.Select(i => string.Format(CultureInfo.CurrentCulture, "{0} ({1:P1})", i.Label, (double)i.Value / total)).ToArray();
            var legendWidth = labels.Select(l => ChartText.Create(l, typeface, Brushes.Black, this).Width).DefaultIfEmpty(0).Max() + LegendBoxSize + 6;
            var diameter = Math.Min(ActualHeight - Padding * 2, ActualWidth - legendWidth - Padding * 3);
            if (diameter <= 0)
            {
                return;
            }
            var radius = diameter / 2;
            var center = new Point(Padding + radius, ActualHeight / 2);
            var outline = new Pen(Brushes.White, 1);
            outline.Freeze();

            // 上から時計回りに並べる
            double startAngle = -90;
            for (int i = 0; i < items.Count; i++)
            {
                var sweep = 360.0 * items[i].Value / total;
                var brush = new SolidColorBrush(items[i].Color);
                brush.Freeze();
                if (sweep >= 360)
                {
                    drawingContext.DrawEllipse(brush, outline, center, radius, radius);
                }
                else if (sweep > 0)
                {
                    drawingContext.DrawGeometry(brush, outline, CreateSlice(center, radius, startAngle, sweep));
                }
                startAngle += sweep;
            }

            var legendX = center.X + radius + Padding * 2;
            var legendY = Math.Max(Padding, ActualHeight / 2 - LegendLineHeight * items.Count / 2);
            for (int i = 0; i < items.Count; i++)
            {
                var brush = new SolidColorBrush(items[i].Color);
                brush.Freeze();
                var y = legendY + LegendLineHeight * i;
                drawingContext.DrawRectangle(brush, null, new Rect(legendX, y + (LegendLineHeight - LegendBoxSize) / 2, LegendBoxSize, LegendBoxSize));
                var text = ChartText.Create(labels[i], typeface, Brushes.Black, this);
                drawingContext.DrawText(text, new Point(legendX + LegendBoxSize + 6, y + (LegendLineHeight - text.Height) / 2));
            }
        }

        private static Geometry CreateSlice(Point center, double radius, double startAngle, double sweep)
        {
            var start = PointOnCircle(center, radius, startAngle);
            var end = PointOnCircle(center, radius, startAngle + sweep);
            var figure = new PathFigure { StartPoint = center, IsClosed = true };
            figure.Segments.Add(new LineSegment(start, true));
            figure.Segments.Add(new ArcSegment(end, new Size(radius, radius), 0, sweep > 180, SweepDirection.Clockwise, true));
            var geometry = new PathGeometry([figure]);
            geometry.Freeze();
            return geometry;
        }

        private static Point PointOnCircle(Point center, double radius, double angle)
        {
            var rad = angle * Math.PI / 180;
            return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
        }
    }

    /// <summary>
    /// 統計で使う、秒ごとの値の棒グラフです。今の時刻の棒を赤くする。
    /// </summary>
    public class TimeColumnChart : FrameworkElement
    {
        const double LeftMargin = 40;
        const double RightMargin = 10;
        const double TopMargin = 10;
        const double BottomMargin = 22;
        const double MinLabelSpacing = 50;

        static readonly Brush BarBrush = Freeze(new SolidColorBrush(Color.FromRgb(65, 140, 240)));
        static readonly Brush HighlightBrush = Brushes.Red;
        static readonly Pen AxisPen = Freeze(new Pen(Brushes.Black, 1));
        static readonly Pen GridPen = Freeze(new Pen(Brushes.LightGray, 1));

        IReadOnlyList<KeyValuePair<int, float>> points = [];
        int? highlightTime;

        /// <summary>
        /// 秒と、その秒の値です。秒の順に並べる。
        /// </summary>
        public IReadOnlyList<KeyValuePair<int, float>> Points
        {
            get { return points; }
            set
            {
                points = value ?? [];
                InvalidateVisual();
            }
        }

        /// <summary>
        /// 赤くする秒です。
        /// </summary>
        public int? HighlightTime
        {
            get { return highlightTime; }
            set
            {
                if (highlightTime != value)
                {
                    highlightTime = value;
                    InvalidateVisual();
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            var plotWidth = ActualWidth - LeftMargin - RightMargin;
            var plotHeight = ActualHeight - TopMargin - BottomMargin;
            if (points.Count == 0 || plotWidth <= 0 || plotHeight <= 0)
            {
                return;
            }

            var typeface = ChartText.Typeface;
            var maxValue = NiceCeiling(points.Max(p => p.Value));
            var bottom = TopMargin + plotHeight;

            // 縦軸の目盛り
            const int YDivision = 5;
            for (int i = 0; i <= YDivision; i++)
            {
                var value = maxValue * i / YDivision;
                var y = Snap(bottom - plotHeight * i / YDivision);
                drawingContext.DrawLine(GridPen, new Point(LeftMargin, y), new Point(LeftMargin + plotWidth, y));
                var text = ChartText.Create(value.ToString("0.##", CultureInfo.CurrentCulture), typeface, Brushes.Black, this);
                drawingContext.DrawText(text, new Point(LeftMargin - text.Width - 4, y - text.Height / 2));
            }

            var barWidth = plotWidth / points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                var height = plotHeight * Math.Max(0, points[i].Value) / maxValue;
                var brush = points[i].Key == highlightTime ? HighlightBrush : BarBrush;
                drawingContext.DrawRectangle(brush, null, new Rect(LeftMargin + barWidth * i, bottom - height, Math.Max(1, barWidth * 0.8), height));
            }

            // 横軸の目盛り（重ならないように間引く）
            var labelStep = Math.Max(1, (int)Math.Ceiling(MinLabelSpacing / barWidth));
            for (int i = 0; i < points.Count; i += labelStep)
            {
                var text = ChartText.Create(new TimeSpan(0, 0, points[i].Key).ToString(@"mm\:ss"), typeface, Brushes.Black, this);
                var x = LeftMargin + barWidth * (i + 0.4);
                drawingContext.DrawText(text, new Point(x - text.Width / 2, bottom + 3));
            }
            drawingContext.DrawLine(AxisPen, new Point(LeftMargin, Snap(bottom)), new Point(LeftMargin + plotWidth, Snap(bottom)));
            drawingContext.DrawLine(AxisPen, new Point(Snap(LeftMargin), TopMargin), new Point(Snap(LeftMargin), bottom));
        }

        /// <summary>
        /// 目盛りが切りのいい値になるように、最大値を 1, 2, 5 × 10 の累乗に切り上げる。
        /// </summary>
        private static float NiceCeiling(float value)
        {
            if (value <= 0)
            {
                return 1;
            }
            var exponent = Math.Pow(10, Math.Floor(Math.Log10(value)));
            foreach (var step in new[] { 1, 2, 5, 10 })
            {
                if (value <= step * exponent)
                {
                    return (float)(step * exponent);
                }
            }
            return (float)(10 * exponent);
        }

        private static double Snap(double value)
        {
            return Math.Floor(value) + 0.5;
        }

        private static T Freeze<T>(T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }
    }

    static class ChartText
    {
        public static Typeface Typeface
        {
            get { return new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal); }
        }

        public static FormattedText Create(string text, Typeface typeface, Brush brush, Visual visual)
        {
            return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12, brush,
                VisualTreeHelper.GetDpi(visual).PixelsPerDip);
        }
    }
}
