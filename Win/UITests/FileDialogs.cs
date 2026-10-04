using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Runtime.InteropServices;

namespace UITests
{
    /// <summary>
    /// ファイルを開く・保存するダイアログ（Windows の共通のダイアログ）を扱います。
    /// </summary>
    /// <remarks>
    /// Windows 11 のダイアログのボタンは、UI Automation では押す機能（パターン）がないことがあるので、
    /// ファイル名の欄に入れてから、ウィンドウに OK のコマンド（WM_COMMAND）を送る。
    /// </remarks>
    public static class FileDialogs
    {
        const uint WM_COMMAND = 0x0111;
        const int IDOK = 1;

        // ファイル名の欄の ID（開くダイアログと、保存するダイアログ）
        static readonly string[] fileNameIds = ["1148", "1001"];

        /// <summary>
        /// ファイル名を入れて、開く（保存する）を押します。
        /// </summary>
        public static void Accept(Window dialog, string path)
        {
            AutomationElement fileName = null;
            foreach (var id in fileNameIds)
            {
                fileName = dialog.FindFirstDescendant(cf => cf.ByAutomationId(id).And(cf.ByControlType(ControlType.Edit)));
                if (fileName != null)
                {
                    break;
                }
            }
            if (fileName == null)
            {
                throw new InvalidOperationException("ファイルのダイアログに、ファイル名の欄がありません。");
            }
            fileName.Patterns.Value.Pattern.SetValue(path);
            if (!PostMessage(dialog.Properties.NativeWindowHandle.Value, WM_COMMAND, (IntPtr)IDOK, IntPtr.Zero))
            {
                throw new InvalidOperationException("ファイルのダイアログの OK を押せませんでした。");
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
