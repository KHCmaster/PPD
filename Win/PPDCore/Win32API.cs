using System.Runtime.InteropServices;

namespace PPDCore
{
    class Win32API
    {
        [DllImport("winmm.dll")]
        public static extern long timeGetTime();
        [DllImport("winmm.dll")]
        public static extern void timeBeginPeriod(int x);
        [DllImport("winmm.dll")]
        public static extern void timeEndPeriod(int x);
    }
}
