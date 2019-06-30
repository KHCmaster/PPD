using System;

namespace BezierDrawControl
{
    public class BezierControlPointEditEventArgs : EventArgs
    {
        public int EditIndex
        {
            get;
            private set;
        }
        public EditType EditType
        {
            get;
            private set;
        }

        public BezierControlPointEditEventArgs(EditType type)
        {
            EditType = type;
            EditIndex = -1;
        }

        public BezierControlPointEditEventArgs(EditType type, int editIndex)
        {
            EditType = type;
            EditIndex = editIndex;
        }
    }
}
