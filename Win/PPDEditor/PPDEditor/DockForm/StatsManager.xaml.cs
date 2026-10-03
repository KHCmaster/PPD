using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PPDEditor
{
    public partial class StatsManager : DockView
    {
        public enum ChartMode
        {
            None = 0,
            Normal = 1,
            Long = 2,
            Both = 3,
        }

        private LayerStats[] currentStats;
        private readonly Dictionary<ButtonType, Color> colorDict = new()
        {
            {ButtonType.Square, Color.FromRgb(231,75,245)},            {ButtonType.Cross, Color.FromRgb(100,185,239)},            {ButtonType.Circle, Color.FromRgb(238,54,80)},            {ButtonType.Triangle, Color.FromRgb(24,208,37)},            {ButtonType.Left, Color.FromRgb(255,65,221)},            {ButtonType.Down, Color.FromRgb(65,208,255)},            {ButtonType.Right, Color.FromRgb(255,65,98)},            {ButtonType.Up, Color.FromRgb(66,254,77)},            {ButtonType.R, Color.FromRgb(181,245,251)},            {ButtonType.L, Color.FromRgb(75,231,245)}        };

        public ChartMode CurrentChartMode
        {
            get
            {
                return comboBox1.SelectedIndex switch
                {
                    0 => ChartMode.Both,
                    1 => ChartMode.Normal,
                    2 => ChartMode.Long,
                    _ => ChartMode.None,
                };
            }
        }

        public StatsManager()
        {
            InitializeComponent();
        }

        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += Seekmain_onmoveseek;
        }

        public void SetLang()
        {
            this.データ更新MenuItem.Header = Utility.Language["UpdateData"];
            this.tabPage1.Header = Utility.Language["Data"];
            this.tabPage2.Header = Utility.Language["Visualization"];
            this.tabPage3.Header = Utility.Language["Difficulty"];
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add(String.Format("{0}+{1}", Utility.Language["NormalMark"], Utility.Language["LongMark"]));
            this.comboBox1.Items.Add(Utility.Language["NormalMark"]);
            this.comboBox1.Items.Add(Utility.Language["LongMark"]);
            comboBox1.SelectedIndex = 0;
        }

        private void RefreshData()
        {
            LayerStats stats = currentStats[listBox1.SelectedIndex];
            var rows = new List<KeyValuePair<string, object>>();
            foreach (var i in WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows)
            {
                rows.Add(new(String.Format("{0}({1}+{2})", GetButtonString(i), Utility.Language["NormalMark"], Utility.Language["LongMark"]),
                    String.Format("{0}({1}+{2})", stats.NormalCounts[i] + stats.LongCounts[i], stats.NormalCounts[i], stats.LongCounts[i])));
            }
            rows.Add(new(Utility.Language["NormalMark"], stats.AllNormalCounts));
            rows.Add(new(Utility.Language["LongMark"], stats.AllLongCounts));
            rows.Add(new(Utility.Language["Sum"], stats.AllCounts));
            rows.Add(new(Utility.Language["Difficulty(Average)"], stats.Result.Average));
            rows.Add(new(Utility.Language["Difficulty(Peak)"], stats.Result.Peak));
            dataGridView1.ItemsSource = rows;
        }

        private void RefreshChart()
        {
            LayerStats stats = currentStats[listBox1.SelectedIndex];
            var items = new List<PieChartItem>();
            for (int i = 0; i < 10; i++)
            {
                switch (CurrentChartMode)
                {
                    case ChartMode.Normal:
                        if (stats.NormalCounts[i] > 0)
                        {
                            items.Add(CreatePieItem(i, stats.NormalCounts[i]));
                        }
                        break;
                    case ChartMode.Long:
                        if (stats.LongCounts[i] > 0)
                        {
                            items.Add(CreatePieItem(i, stats.LongCounts[i]));
                        }
                        break;
                    case ChartMode.Both:
                        if (stats.NormalCounts[i] + stats.LongCounts[i] > 0)
                        {
                            items.Add(CreatePieItem(i, stats.NormalCounts[i] + stats.LongCounts[i]));
                        }
                        break;
                }
            }

            chart1.Items = items;
        }

        private PieChartItem CreatePieItem(int i, int value)
        {
            var color = colorDict[(ButtonType)i];
            return new PieChartItem(GetButtonString(i), value, Color.FromArgb(200, color.R, color.G, color.B));
        }

        private void RefreshDifficultyChart()
        {
            LayerStats stats = currentStats[listBox1.SelectedIndex];
            var points = new List<KeyValuePair<int, float>>();
            for (int i = (int)Math.Floor(WindowUtility.IniFileWriter.StartTime); i <= (int)Math.Ceiling(WindowUtility.IniFileWriter.EndTime); i++)
            {
                points.Add(new(i, stats.Result.Data.TryGetValue(i, out float value) ? value : 0));
            }
            chart2.Points = points;
        }

        private static string GetButtonString(int i)
        {
            return Utility.Language[((ButtonType)i).ToString()];
        }

        void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentStats == null)
            {
                return;
            }

            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < currentStats.Length)
            {
                RefreshChart();
            }
        }

        private void データ更新MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var statsList = new List<LayerStats>();
            foreach (LayerDisplay ld in WindowUtility.LayerManager.AllLayerDisplay)
            {
                var markDatas = ld.PPDData.GetSortedDataAsMarkData();
                var stats = new LayerStats(ld.DisplayName,
                    ScoreDifficultyMeasure.Measure(markDatas));
                for (int i = 0; i < 10; i++)
                {
                    foreach (KeyValuePair<float, Mark> kvp in ld.PPDData.Data[i])
                    {
                        if (kvp.Value is ExMark)
                        {
                            stats.LongCounts[i]++;
                        }
                        else
                        {
                            stats.NormalCounts[i]++;
                        }
                    }
                }
                statsList.Add(stats);
            }

            var allMarkDatas = WindowUtility.LayerManager.GetAllData(true);
            var allStats = new LayerStats(Utility.Language["AllLayers"],
                 ScoreDifficultyMeasure.Measure(allMarkDatas));
            for (int i = 0; i < 10; i++)
            {
                allStats.NormalCounts[i] = statsList.Sum(stats => stats.NormalCounts[i]);
                allStats.LongCounts[i] = statsList.Sum(stats => stats.LongCounts[i]);
            }
            statsList.Insert(0, allStats);
            currentStats = [.. statsList];

            listBox1.Items.Clear();
            foreach (LayerStats stats in currentStats)
            {
                listBox1.Items.Add(stats.Name);
            }

            listBox1.SelectedIndex = 0;
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentStats == null)
            {
                return;
            }

            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < currentStats.Length)
            {
                RefreshData();
                RefreshChart();
                RefreshDifficultyChart();
            }
        }

        void Seekmain_onmoveseek(object sender, EventArgs e)
        {
            chart2.HighlightTime = (int)WindowUtility.Seekmain.Currenttime;
        }

        class LayerStats(string name, ScoreDifficultyMeasureResult result)
        {
            readonly int[] normalCounts = new int[10];
            readonly int[] longCounts = new int[10];

            public string Name
            {
                get;
                private set;
            } = name;

            public int[] NormalCounts
            {
                get
                {
                    return normalCounts;
                }
            }

            public int[] LongCounts
            {
                get
                {
                    return longCounts;
                }
            }

            public int AllCounts
            {
                get
                {
                    return normalCounts.Sum() + longCounts.Sum();
                }
            }

            public int AllNormalCounts
            {
                get
                {
                    return normalCounts.Sum();
                }
            }

            public int AllLongCounts
            {
                get
                {
                    return longCounts.Sum();
                }
            }

            public ScoreDifficultyMeasureResult Result
            {
                get;
                private set;
            } = result;
        }
    }
}
