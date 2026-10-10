using PPDCoreModel.Data;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class MarkResults
    {
        readonly HashSet<EffectType> results;

        public bool this[EffectType result]
        {
            get
            {
                return results.Contains(result);
            }
        }

        public EffectType First
        {
            get
            {
                return results.First();
            }
        }

        public MarkResults()
        {
            results = [];
        }

        public void Add(EffectType result)
        {
            results.Add(result);
        }
    }
}
