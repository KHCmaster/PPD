using System.Collections.Generic;

namespace PPDEditor
{
    class SameTimingMarks : IEnumerable<Mark>
    {
        private List<Mark> marks;

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

        public SameTimingMarks()
        {
            marks = new List<Mark>();
        }

        public void Add(Mark mark)
        {
            marks.Add(mark);
            SameTimings |= 1 << (int)mark.Type;
        }

        public void Clear()
        {
            marks.Clear();
            SameTimings = 0;
        }

        #region IEnumerable<Mark> メンバー

        public IEnumerator<Mark> GetEnumerator()
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
