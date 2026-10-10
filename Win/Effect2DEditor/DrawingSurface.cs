using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Effect2DEditor
{
    /// <summary>
    /// 自分で描く領域です（以前の WinForms の CustomUserControl の代わり）。描く処理は <see cref="Render"/> に渡します。
    /// </summary>
    /// <remarks>
    /// 線は以前の GDI+ と同じようにくっきり描くため、アンチエイリアスを切っています。
    /// </remarks>
    public class DrawingSurface : FrameworkElement
    {
        const double FontSize = 12;

        public static readonly Pen BlackPen = CreatePen(Brushes.Black);
        public static readonly Pen SilverPen = CreatePen(Brushes.Silver);
        public static readonly Pen LightGrayPen = CreatePen(Brushes.LightGray);
        public static readonly Pen RedPen = CreatePen(Brushes.Red);

        private static Pen CreatePen(Brush brush)
        {
            var pen = new Pen(brush, 1);
            pen.Freeze();
            return pen;
        }

        static readonly Typeface typeface = new(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        /// <summary>
        /// 描く処理です。
        /// </summary>
        public Action<DrawingContext> Render
        {
            get;
            set;
        }

        /// <summary>
        /// 背景の色です。
        /// </summary>
        public Brush Background
        {
            get;
            set;
        } = SystemColors.ControlBrush;

        public DrawingSurface()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            ClipToBounds = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // 何も描いていないところでもマウスを受け取れるように、背景を塗る
            drawingContext.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
            Render?.Invoke(drawingContext);
        }

        /// <summary>
        /// 枠を描きます（GDI+ の DrawRectangle と同じく、幅と高さは線の中心どうしの間隔）。
        /// </summary>
        public static void DrawFrame(DrawingContext dc, Pen pen, double x, double y, double width, double height)
        {
            dc.DrawRectangle(null, pen, new Rect(x + 0.5, y + 0.5, Math.Max(0, width), Math.Max(0, height)));
        }

        /// <summary>
        /// 線を描きます。
        /// </summary>
        public static void DrawLine(DrawingContext dc, Pen pen, double x1, double y1, double x2, double y2)
        {
            dc.DrawLine(pen, new Point(x1 + 0.5, y1 + 0.5), new Point(x2 + 0.5, y2 + 0.5));
        }

        /// <summary>
        /// 文字を描きます。<paramref name="maxWidth"/> を超えるときは末尾を省略します。
        /// </summary>
        public void DrawText(DrawingContext dc, string text, Brush brush, double x, double y, double maxWidth = double.NaN)
        {
            var formattedText = new FormattedText(text ?? "", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, FontSize, brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            if (!double.IsNaN(maxWidth))
            {
                formattedText.MaxTextWidth = Math.Max(1, maxWidth);
                formattedText.MaxLineCount = 1;
                formattedText.Trimming = TextTrimming.CharacterEllipsis;
            }
            dc.DrawText(formattedText, new Point(x, y));
        }

        /// <summary>
        /// 文字の高さです（以前の Font.Height の代わり）。
        /// </summary>
        public static double TextHeight
        {
            get { return Math.Ceiling(FontSize * typeface.FontFamily.LineSpacing); }
        }

        /// <summary>
        /// 描き直します。
        /// </summary>
        public void DrawAndRefresh()
        {
            InvalidateVisual();
        }
    }
}
