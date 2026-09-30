using System.Runtime.InteropServices;

namespace PPDCore
{
    class NativeMethods
    {
        [DllImport("winmm.dll")]
        public static extern long timeGetTime();
        [DllImport("winmm.dll")]
        public static extern void timeBeginPeriod(int x);
        [DllImport("winmm.dll")]
        public static extern void timeEndPeriod(int x);
    }
}
