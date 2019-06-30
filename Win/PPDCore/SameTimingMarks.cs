using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    class SameTimingMarks : IEnumerable<MarkDataBase>
    {
        private List<MarkDataBase> marks;

        public int SameTimings
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                return marks.Count;
            }
        }

        public MarkDataBase this[int index]
        {
            get
            {
                return marks[index];
            }
        }

        public SameTimingMarks()
        {
            marks = new List<MarkDataBase>();
        }

        public void Add(MarkDataBase mark)
        {
            marks.Add(mark);
            SameTimings |= 1 << (int)mark.ButtonType;
        }

        public void Remove(MarkDataBase mark)
        {
            marks.Remove(mark);
            SameTimings &= ~(1 << (int)mark.ButtonType);
        }

        public bool Contains(ButtonType buttonType)
        {
            return ((SameTimings >> (int)buttonType) & 1) == 1;
        }

        public bool IsSlideButton(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Triangle:
                case ButtonType.Circle:
                    int mask = (1 << (int)ButtonType.R) | (1 << (int)ButtonType.L);
                    if ((SameTimings & mask) > 0)
                    {
                        return true;
                    }
                    break;
                case ButtonType.R:
                    return true;
                case ButtonType.L:
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            marks.Clear();
            SameTimings = 0;
        }

        public void Sort(ButtonType[] initializeOrder)
        {
            MarkComparer.Comparer.Table = initializeOrder;
            marks.Sort(MarkComparer.Comparer);
        }

        class MarkComparer : IComparer<MarkDataBase>
        {
            public static MarkComparer Comparer = new MarkComparer();
            #region IComparer<IPPDData> メンバ

            public int Compare(MarkDataBase x, MarkDataBase y)
            {
                return -(Array.IndexOf(Table, x.ButtonType) - Array.IndexOf(Table, y.ButtonType));
            }

            public ButtonType[] Table
            {
                get;
                set;
            }

            #endregion
        }

        #region IEnumerable<IPPDData> メンバー

        public IEnumerator<MarkDataBase> GetEnumerator()
        {
            return marks.GetEnumerator();
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return marks.GetEnumerator();
        }

        #endregion
    }
}
