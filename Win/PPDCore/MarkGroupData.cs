using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class MarkGroupData(float time, MarkDataBase[] data) : IGrouping<float, MarkDataBase>
    {
        private readonly MarkDataBase[] data = data;

        public SameTimingMarks SameTimings
        {
            get;
            private set;
        } = [.. data];

        #region IGrouping<float,IPPDData> メンバー

        public float Key
        {
            get;
            private set;
        } = time;

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
