using AvalonDock.Layout;
using System.Windows.Input;

namespace PPDEditor.Forms
{
    /// <summary>
    /// ドックのウィンドウ（AvalonDock）を扱う処理です。
    /// </summary>
    static class DockUtility
    {
        /// <summary>
        /// ウィンドウを出すか隠す。出ていて前にあるときは隠し、ほかのウィンドウの後ろにあるときは前に出す。
        /// Shift を押していると浮かせて出す（以前と同じ）。
        /// </summary>
        public static void ShowOrHideWindow(LayoutAnchorable anchorable)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                anchorable.Show();
                anchorable.Float();
                anchorable.IsActive = true;
                return;
            }
            if (!anchorable.IsVisible || anchorable.IsAutoHidden)
            {
                anchorable.Show();
                anchorable.IsActive = true;
            }
            else if (anchorable.IsActive || anchorable.IsFloating)
            {
                anchorable.Hide();
            }
            else
            {
                anchorable.IsActive = true;
            }
        }

        /// <summary>
        /// ウィンドウが出ているかどうか（自動で隠れているときは出ていないとする）。
        /// </summary>
        public static bool IsShown(LayoutAnchorable anchorable)
        {
            return anchorable.IsVisible && !anchorable.IsAutoHidden;
        }
    }
}
