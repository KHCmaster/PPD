using System;

namespace PPDMovie
{
    public static class MovieUtility
    {
        public static IntPtr Window
        {
            get;
            set;
        }

        public static event EventHandler GraphNotify;
        public static void OnGraphNotify()
        {
            if (GraphNotify != null)
            {
                GraphNotify.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
