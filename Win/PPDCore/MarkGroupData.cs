using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class MarkGroupData : IGrouping<float, MarkDataBase>
    {
        private MarkDataBase[] data;

        public SameTimingMarks SameTimings
        {
            get;
            private set;
        }

        public MarkGroupData(float time, MarkDataBase[] data)
        {
            Key = time;
            this.data = data;
            SameTimings = new SameTimingMarks();
            foreach (var ppdData in data)
            {
                SameTimings.Add(ppdData);
            }
        }

        #region IGrouping<float,IPPDData> メンバー

        public float Key
        {
            get;
            private set;
        }

        #endregion

        #region IEnumerable<IPPDData> メンバー

        public IEnumerator<MarkDataBase> GetEnumerator()
        {
            foreach (var d in data)
            {
                yield return d;
            }
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        #endregion
    }
}
