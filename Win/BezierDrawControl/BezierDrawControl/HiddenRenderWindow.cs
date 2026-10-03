using System;
using System.Runtime.InteropServices;

namespace BezierDrawControl
{
    /// <summary>
    /// DirectX のスワップチェーンを作るための、表示しないウィンドウです。
    /// 描画した結果はバックバッファーから読み出して WPF の画面に表示します（WPF に HWND を埋め込まないので、ドッキングやスクロールの制約を受けない）。
    /// </summary>
    sealed class HiddenRenderWindow : IDisposable
    {
        public IntPtr Handle
        {
            get;
            private set;
        }

        public HiddenRenderWindow(int width, int height)
        {
            Handle = NativeMethods.CreateWindowExW(0, "STATIC", "", NativeMethods.WS_POPUP, 0, 0, width, height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create the render window: " + Marshal.GetLastWin32Error());
            }
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }

    static class NativeMethods
    {
        internal const uint WS_POPUP = 0x80000000;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateWindowExW(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(IntPtr hWnd);
    }
}
