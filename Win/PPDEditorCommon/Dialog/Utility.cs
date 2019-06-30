using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace PPDEditorCommon.Dialog
{
    class Utility
    {
        private const int GWL_STYLE = -16,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        internal static void HideMinimizeAndMaximizeButtons(Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }

        public static bool CheckValidFileName(string fileName)
        {
            foreach (Char c in Path.GetInvalidFileNameChars())
            {
                if (fileName.IndexOf(c) >= 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
