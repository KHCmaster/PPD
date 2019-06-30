using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDFramework
{
    /// <summary>
    /// 譜面の難度を測定するクラスです。
    /// </summary>
    public class ScoreDifficultyMeasure
    {
        private ScoreDifficultyMeasureResult MeasureNormal(string path)
        {
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                return Measure(PPDReader.Read(stream));
            }
        }

        /// <summary>
        /// 難度を測定します。
        /// </summary>
        /// <param name="ppdData">譜面データ。</param>
        /// <returns>難度。</returns>
        public static ScoreDifficultyMeasureResult Measure(MarkDataBase[] ppdData)
        {
            if (ppdData == null || ppdData.Length == 0)
            {
                return new ScoreDifficultyMeasureResult(new SortedDictionary<int, float>());
            }
            return MeasureImpl(ppdData);
        }

        /// <summary>
        /// 計測結果の生データを返します。
        /// </summary>
        /// <param name="ppdData">譜面データ。</param>
        /// <returns>生データ。</returns>
        private static ScoreDifficultyMeasureResult MeasureImpl(MarkDataBase[] ppdData)
        {
            if (ppdData.Length == 0)
            {
                return new ScoreDifficultyMeasureResult(new SortedDictionary<int, float>());
            }

            var previousPresses = new List<bool[]>();
            float fLastTime = 0;
            int lastTime = 0;
            float sum = 0;
            var points = new SortedDictionary<int, float>();
            foreach (KeyValuePair<float, MarkDataBase[]> pair in ppdData.GroupBy(p => p.Time).ToDictionary(g => g.Key, g => g.ToArray()))
            {
                if (lastTime != (int)pair.Key)
                {
                    points.Add(lastTime, sum);
                    sum = 0;
                }
                if (pair.Key - fLastTime >= 1f)
                {
                    previousPresses.Clear();
                }
                bool[] presses = new bool[10];
                foreach (MarkDataBase p in pair.Value)
                {
                    presses[(int)p.ButtonType] = true;
                }
                float basePoint = Math.Min(11, previousPresses.Count + 1);
                for (int i = previousPresses.Count - 1; i >= Math.Max(0, previousPresses.Count - 10); i--)
                {
                    int iter = previousPresses.Count - 1 - i;
                    bool same = true;
                    for (int j = 0; j < presses.Length; j++)
                    {
                        if (presses[j] != previousPresses[i][j])
                        {
                            same = false;
                            break;
                        }
                    }
                    if (same)
                    {
                        basePoint -= (float)Math.Pow(1.1f, iter);
                    }
                }
                if (basePoint < 0)
                {
                    basePoint = 0;
                }
                sum += basePoint + pair.Value.Length / 2f;
                previousPresses.Add(presses);
                fLastTime = pair.Key;
                lastTime = (int)pair.Key;
            }
            if (sum != 0)
            {
                points.Add(lastTime, sum);
            }
            return new ScoreDifficultyMeasureResult(points);
        }
    }
}
