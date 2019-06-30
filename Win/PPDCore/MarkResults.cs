using PPDCoreModel.Data;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class MarkResults
    {
        HashSet<EffectType> results;

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
            results = new HashSet<EffectType>();
        }

        public void Add(EffectType result)
        {
            results.Add(result);
        }
    }
}
