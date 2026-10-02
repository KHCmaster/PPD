using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PPDInstaller
{
    class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        const int CSIDL_COMMON_PROGRAMS = 0x17;

        public static string GetCommonStartmenuPath()
        {
            var path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_PROGRAMS, false);
            return path.ToString();
        }

        [DllImport("kernel32.dll")]
        internal static extern int GetACP();

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        const int GWL_STYLE = -16;
        const long WS_SYSMENU = 0x00080000;

        /// <summary>
        /// タイトルバーのボタン（閉じるボタンなど）を消します（WinForms の ControlBox = false と同じ）。
        /// </summary>
        public static void HideSystemMenu(System.Windows.Window window)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style & ~WS_SYSMENU));
        }
    }
}
