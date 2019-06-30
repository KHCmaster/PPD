using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class LatencyFixer
    {
        List<float> diffs;
        int initialLatencyCount;

        public bool Enabled
        {
            get;
            set;
        }

        public LatencyFixer(int initialLatencyCount)
        {
            Enabled = true;
            this.initialLatencyCount = initialLatencyCount;
            diffs = new List<float>();
        }

        public void Add(float diff)
        {
            diffs.Add(diff);
        }

        public int GetNewLatencyCount()
        {
            var newCalc = (int)(-Calc() * 1000);
            var ratio = Math.Min(1, diffs.Count / 25f);
            return (int)(initialLatencyCount * (1 - ratio) + newCalc * ratio);
        }

        private float Calc()
        {
            if (diffs.Count > 0)
            {
                return diffs.Average();
            }
            return 0;
        }
    }
}
