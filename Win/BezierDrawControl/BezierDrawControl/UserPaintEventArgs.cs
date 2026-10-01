using System;

namespace BezierDrawControl
{
    public class UserPaintEventArgs(IBezierDrawContext context) : EventArgs
    {
        public IBezierDrawContext Context
        {
            get;
            private set;
        } = context;
    }
}
