using System;

namespace PPDTest
{
    public static class Utility
    {
        public static event EventHandler GraphNotify;
        public static IntPtr window;
        public static void OnGraphNotify()
        {
            GraphNotify?.Invoke(null, EventArgs.Empty);
        }
    }
}
