using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UITests
{
    /// <summary>
    /// アプリのウィンドウを Win32 で調べます。
    /// </summary>
    /// <remarks>
    /// UI Automation でウィンドウを調べると、アプリの UI のスレッドに問い合わせるので、
    /// UI のスレッドが忙しいアプリ（PPDEditor のゲームのループなど）では時間がかかる。Win32 ならすぐに分かる。
    /// </remarks>
    static class NativeWindows
    {
        const uint GW_OWNER = 4;

        /// <summary>
        /// プロセスの、見えているトップレベルのウィンドウ（所有者のあるものも含む）です。
        /// </summary>
        public static List<IntPtr> OfProcess(int processId)
        {
            var result = new List<IntPtr>();
            EnumWindows((handle, _) =>
            {
                GetWindowThreadProcessId(handle, out var pid);
                if (pid == processId && IsWindowVisible(handle))
                {
                    result.Add(handle);
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        public static string GetTitle(IntPtr handle)
        {
            var builder = new StringBuilder(512);
            GetWindowText(handle, builder, builder.Capacity);
            return builder.ToString();
        }

        public static IntPtr GetOwner(IntPtr handle)
        {
            return GetWindow(handle, GW_OWNER);
        }

        public static bool IsEnabled(IntPtr handle)
        {
            return IsWindowEnabled(handle);
        }

        /// <summary>
        /// タイトルバーのあるウィンドウかどうか（ポップアップのメニューなどを除くため）。
        /// </summary>
        public static bool HasCaption(IntPtr handle)
        {
            const int GWL_STYLE = -16;
            const long WS_CAPTION = 0x00C00000;
            return (GetWindowLongPtr(handle, GWL_STYLE).ToInt64() & WS_CAPTION) == WS_CAPTION;
        }

        /// <summary>
        /// ウィンドウを閉じます（閉じるボタンと同じ。ダイアログではキャンセルになる）。
        /// </summary>
        public static void Close(IntPtr handle)
        {
            const uint WM_CLOSE = 0x0010;
            PostMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        delegate bool EnumWindowsProc(IntPtr handle, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr handle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowEnabled(IntPtr handle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr handle, StringBuilder text, int maxCount);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr handle, uint command);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLongPtr(IntPtr handle, int index);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
    }
}
