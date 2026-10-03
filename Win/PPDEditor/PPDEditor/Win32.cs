using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PPDEditor
{
    class NativeMethods
    {
        [DllImport("winmm.dll")]
        internal static extern long timeGetTime();

        [DllImport("winmm.dll")]
        internal static extern void timeBeginPeriod(int x);

        [DllImport("winmm.dll")]
        internal static extern void timeEndPeriod(int x);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        internal static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        internal static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    }
}
