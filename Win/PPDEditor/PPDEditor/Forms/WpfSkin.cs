using System.Windows.Media;

namespace PPDEditor.Forms
{
    /// <summary>
    /// スキン（<see cref="PPDEditorSkin"/>）の色を WPF のブラシにします。
    /// </summary>
    static class WpfSkin
    {
        /// <summary>
        /// 背景の上に書く文字の色です。
        /// </summary>
        public static Brush TransparentTextBrush
        {
            get { return ToBrush(PPDEditorSkin.Skin.TransparentTextColor); }
        }

        public static SolidColorBrush ToBrush(System.Drawing.Color color)
        {
            var brush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            brush.Freeze();
            return brush;
        }
    }
}
