using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BezierDrawControl
{
    /// <summary>
    /// WPF の描画（<see cref="DrawingContext"/>）でベジエを編集するコントロールです。DirectX は使わない。
    /// </summary>
    public class WpfBezierControl : FrameworkElement
    {
        public class Context(DrawingContext drawingContext) : IBezierDrawContext
        {
            /// <summary>
            /// 1 ピクセルの線がぼやけないように、座標を画素の中心へずらす量です。
            /// </summary>
            const double HalfPixel = 0.5;

            public DrawingContext DrawingContext
            {
                get;
                private set;
            } = drawingContext;

            public void DrawString(string text, System.Drawing.Color color, float fontSize, System.Drawing.PointF point)
            {
                // fontSize は以前の GDI と同じくポイント
                var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                    fontSize * 96 / 72, ToBrush(color), 1);
                DrawingContext.DrawText(formattedText, new Point(point.X, point.Y));
            }

            public void DrawLines(System.Drawing.Color color, System.Drawing.PointF[] points)
            {
                if (points.Length < 2)
                {
                    return;
                }
                var geometry = new StreamGeometry();
                using (var geometryContext = geometry.Open())
                {
                    geometryContext.BeginFigure(ToPoint(points[0]), false, false);
                    for (int i = 1; i < points.Length; i++)
                    {
                        geometryContext.LineTo(ToPoint(points[i]), true, false);
                    }
                }
                geometry.Freeze();
                DrawingContext.DrawGeometry(null, ToPen(color), geometry);
            }

            public void DrawLine(System.Drawing.Color color, System.Drawing.PointF p1, System.Drawing.PointF p2)
            {
                DrawingContext.DrawLine(ToPen(color), ToPoint(p1), ToPoint(p2));
            }

            public void DrawEllipse(System.Drawing.Color color, System.Drawing.RectangleF rect)
            {
                DrawingContext.DrawEllipse(null, ToPen(color),
                    new Point(rect.X + rect.Width / 2 + HalfPixel, rect.Y + rect.Height / 2 + HalfPixel), rect.Width / 2, rect.Height / 2);
            }

            public void DrawRectangle(System.Drawing.Color color, float x, float y, float width, float height)
            {
                DrawingContext.DrawRectangle(null, ToPen(color), new Rect(x + HalfPixel, y + HalfPixel, width, height));
            }

            public void FillRectangle(System.Drawing.Color color, float x, float y, float width, float height)
            {
                DrawingContext.DrawRectangle(ToBrush(color), null, new Rect(x, y, width, height));
            }

            private static Point ToPoint(System.Drawing.PointF point)
            {
                return new Point(point.X + HalfPixel, point.Y + HalfPixel);
            }

            private static SolidColorBrush ToBrush(System.Drawing.Color color)
            {
                var brush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                brush.Freeze();
                return brush;
            }

            private static Pen ToPen(System.Drawing.Color color)
            {
                var pen = new Pen(ToBrush(color), 1);
                pen.Freeze();
                return pen;
            }
        }

        bool antialiasEnabled;

        /// <summary>
        /// 線を滑らかに描くかどうかを取得、設定します。
        /// </summary>
        public bool AntialiasEnabled
        {
            get
            {
                return antialiasEnabled;
            }
            set
            {
                antialiasEnabled = value;
                RenderOptions.SetEdgeMode(this, value ? EdgeMode.Unspecified : EdgeMode.Aliased);
                InvalidateVisual();
            }
        }

        public BezierController Controller
        {
            get;
            private set;
        }

        public WpfBezierControl()
        {
            Controller = new BezierController();
            Controller.DrawRequired += Controller_DrawRequired;
            Focusable = true;
            ClipToBounds = true;
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        private void Controller_DrawRequired()
        {
            DrawAndRefresh();
        }

        /// <summary>
        /// 描き直しを予約します。WPF が次に画面を更新するときに 1 回だけ描く。
        /// </summary>
        public void DrawAndRefresh()
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            Controller.Draw(new Context(drawingContext));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.ChangedButton == MouseButton.Left)
            {
                CaptureMouse();
            }
            var pos = e.GetPosition(this);
            Controller.MouseDown((float)pos.X, (float)pos.Y, e.ChangedButton == MouseButton.Left, WpfCursors.ToBezierModifierKeys(Keyboard.Modifiers));
            DrawAndRefresh();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            Controller.MouseUp();
            DrawAndRefresh();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = e.GetPosition(this);
            var cursor = Controller.MouseMove((float)pos.X, (float)pos.Y, WpfCursors.ToBezierModifierKeys(Keyboard.Modifiers));
            if (cursor.HasValue)
            {
                Cursor = WpfCursors.Get(cursor.Value);
            }
        }
    }
}
