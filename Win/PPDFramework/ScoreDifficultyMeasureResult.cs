using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDFramework
{
    /// <summary>
    /// 難易度測定の結果です。
    /// </summary>
    public class ScoreDifficultyMeasureResult
    {
        private const int PeakSetCount = 8;

        /// <summary>
        /// 生のデータを取得します。
        /// </summary>
        public SortedDictionary<int, float> Data
        {
            get;
            private set;
        }

        /// <summary>
        /// 平均を取得します。
        /// </summary>
        public float Average
        {
            get;
            private set;
        }

        /// <summary>
        /// ピーク値を取得します。
        /// </summary>
        public float Peak
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="data">データ。</param>
        internal ScoreDifficultyMeasureResult(SortedDictionary<int, float> data)
        {
            Data = data;
            Update();
        }

        private void Update()
        {
            if (Data.Count == 0)
            {
                Average = 0;
            }
            else
            {
                Average = (float)Data.Values.Sum() / Data.Count;
            }
            var prevs = new List<float>();
            foreach (KeyValuePair<int, float> pair in Data)
            {
                Peak = Math.Max(Peak, (prevs.Sum() + pair.Value) / (prevs.Count() + 1));
                prevs.Add(pair.Value);
                if (prevs.Count >= PeakSetCount)
                {
                    prevs.RemoveAt(0);
                }
            }
        }
    }
}
