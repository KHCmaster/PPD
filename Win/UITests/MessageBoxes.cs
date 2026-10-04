using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UITests
{
    /// <summary>
    /// メッセージボックス（Win32 の MessageBox）のボタンです。値はボタンのコマンドの ID。
    /// </summary>
    public enum MessageBoxButton
    {
        Ok = 1,
        Cancel = 2,
        Yes = 6,
        No = 7,
    }

    /// <summary>
    /// メッセージボックスを扱います。
    /// </summary>
    /// <remarks>
    /// Windows 11 のメッセージボックスのボタンは、UI Automation では押す機能（パターン）のない Pane になるので、
    /// ウィンドウにボタンのコマンド（WM_COMMAND）を送って押す。
    /// </remarks>
    public static class MessageBoxes
    {
        const uint WM_COMMAND = 0x0111;

        /// <summary>
        /// メッセージボックスの文字を返します。
        /// </summary>
        public static string GetText(Window messageBox)
        {
            return string.Join("\n", messageBox.FindAllDescendants(cf => cf.ByControlType(ControlType.Text)).Select(t => t.Name));
        }

        /// <summary>
        /// メッセージボックスのボタンを押します。
        /// </summary>
        public static void Press(Window messageBox, MessageBoxButton button)
        {
            if (!PostMessage(messageBox.Properties.NativeWindowHandle.Value, WM_COMMAND, (IntPtr)(int)button, IntPtr.Zero))
            {
                throw new InvalidOperationException("メッセージボックスのボタンを押せませんでした。");
            }
        }

        /// <summary>
        /// メッセージボックスの文字を返して、閉じます（OK だけのメッセージ用）。
        /// </summary>
        public static string ReadAndClose(Window messageBox)
        {
            var text = GetText(messageBox);
            messageBox.Close();
            return text;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
