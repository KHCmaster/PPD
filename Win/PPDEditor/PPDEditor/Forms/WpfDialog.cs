using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// WPF のダイアログを表示します（メインウィンドウが WinForms の間に使う）。
    /// </summary>
    static class WpfDialog
    {
        /// <summary>
        /// 開いているウィンドウを所有者にして、モーダルで表示します。
        /// </summary>
        /// <returns>OK で閉じたときは true。</returns>
        public static bool ShowDialog(Window window)
        {
            SetOwner(window);
            return window.ShowDialog() == true;
        }

        /// <summary>
        /// 開いているウィンドウ（なければメインのウィンドウ）を所有者にします。所有者があるときは、その中央に表示します。
        /// </summary>
        public static void SetOwner(Window window)
        {
            Window owner = null;
            if (Application.Current != null)
            {
                foreach (Window candidate in Application.Current.Windows)
                {
                    if (candidate != window && candidate.IsActive && candidate.IsVisible)
                    {
                        owner = candidate;
                        break;
                    }
                }
            }
            owner ??= WindowUtility.MainForm;
            if (owner != null && owner != window && owner.IsVisible)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }
    }
}
