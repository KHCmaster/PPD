using PPDEditor.Command.PPDSheet;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditor.Models
{
    /// <summary>
    /// レイヤー（またはすべてのレイヤー）のマークの数と難易度です。
    /// </summary>
    public class LayerStats(string name, ScoreDifficultyMeasureResult result)
    {
        public string Name { get; } = name;

        /// <summary>
        /// ボタンごとの普通のマークの数です。
        /// </summary>
        public int[] NormalCounts { get; } = new int[10];

        /// <summary>
        /// ボタンごとの長押しのマークの数です。
        /// </summary>
        public int[] LongCounts { get; } = new int[10];

        public int AllNormalCounts
        {
            get { return NormalCounts.Sum(); }
        }

        public int AllLongCounts
        {
            get { return LongCounts.Sum(); }
        }

        public int AllCounts
        {
            get { return AllNormalCounts + AllLongCounts; }
        }

        public ScoreDifficultyMeasureResult Result { get; } = result;

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// 1 つのレイヤーの統計を求めます。
        /// </summary>
        public static LayerStats FromSheet(string name, PPDSheet sheet)
        {
            var stats = new LayerStats(name, ScoreDifficultyMeasure.Measure(sheet.GetSortedDataAsMarkData()));
            for (int i = 0; i < 10; i++)
            {
                foreach (KeyValuePair<float, Mark> pair in sheet.Data[i])
                {
                    if (pair.Value is ExMark)
                    {
                        stats.LongCounts[i]++;
                    }
                    else
                    {
                        stats.NormalCounts[i]++;
                    }
                }
            }
            return stats;
        }

        /// <summary>
        /// すべてのレイヤーの統計を求めます（数はレイヤーの合計、難易度はすべてのマークから測る）。
        /// </summary>
        public static LayerStats Sum(string name, MarkData[] allMarks, IReadOnlyList<LayerStats> layers)
        {
            var stats = new LayerStats(name, ScoreDifficultyMeasure.Measure(allMarks));
            for (int i = 0; i < 10; i++)
            {
                stats.NormalCounts[i] = layers.Sum(s => s.NormalCounts[i]);
                stats.LongCounts[i] = layers.Sum(s => s.LongCounts[i]);
            }
            return stats;
        }
    }
}
