using System.Windows;
using System.Windows.Interop;

namespace PPDEditor.Forms
{
    /// <summary>
    /// WPF のダイアログを表示します（メインウィンドウが WinForms の間に使う）。
    /// </summary>
    static class WpfDialog
    {
        /// <summary>
        /// 開いている WinForms のウィンドウを所有者にして、モーダルで表示します。
        /// </summary>
        /// <returns>OK で閉じたときは true。</returns>
        public static bool ShowDialog(Window window)
        {
            SetOwner(window);
            return window.ShowDialog() == true;
        }

        /// <summary>
        /// 開いている WinForms のウィンドウを所有者にします。所有者があるときは、その中央に表示します。
        /// </summary>
        public static void SetOwner(Window window)
        {
            var owner = System.Windows.Forms.Form.ActiveForm ?? WindowUtility.MainForm;
            if (owner != null && !owner.IsDisposed)
            {
                new WindowInteropHelper(window).Owner = owner.Handle;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }
    }
}
