using System;

namespace BezierDrawControl
{
    public class UserPaintEventArgs : EventArgs
    {
        public IBezierDrawContext Context
        {
            get;
            private set;
        }

        public UserPaintEventArgs(IBezierDrawContext context)
        {
            Context = context;
        }
    }
}
