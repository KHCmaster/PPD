using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace PPDEditor
{
    public partial class StatsManager : ChangableDockContent
    {
        public enum ChartMode
        {
            None = 0,
            Normal = 1,
            Long = 2,
            Both = 3,
        }

        private LayerStats[] currentStats;
        private Dictionary<ButtonType, Color> colorDict = new Dictionary<ButtonType, Color>
        {
            {ButtonType.Square, Color.FromArgb(231,75,245)},            {ButtonType.Cross, Color.FromArgb(100,185,239)},            {ButtonType.Circle, Color.FromArgb(238,54,80)},            {ButtonType.Triangle, Color.FromArgb(24,208,37)},            {ButtonType.Left, Color.FromArgb(255,65,221)},            {ButtonType.Down, Color.FromArgb(65,208,255)},            {ButtonType.Right, Color.FromArgb(255,65,98)},            {ButtonType.Up, Color.FromArgb(66,254,77)},            {ButtonType.R, Color.FromArgb(181,245,251)},            {ButtonType.L, Color.FromArgb(75,231,245)}        };

        public ChartMode CurrentChartMode
        {
            get
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        return ChartMode.Both;
                    case 1:
                        return ChartMode.Normal;
                    case 2:
                        return ChartMode.Long;
                }

                return ChartMode.None;
            }
        }

        public StatsManager()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        }

        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += Seekmain_onmoveseek;
        }

        public void SetLang()
        {
            this.データ更新ToolStripMenuItem.Text = Utility.Language["UpdateData"];
            this.tabPage1.Text = Utility.Language["Data"];
            this.tabPage2.Text = Utility.Language["Visualization"];
            this.tabPage3.Text = Utility.Language["Difficulty"];
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.AddRange(new string[] {
                String.Format("{0}+{1}",Utility.Language["NormalMark"],Utility.Language["LongMark"]),
                Utility.Language["NormalMark"],
                Utility.Language["LongMark"]
            });
            comboBox1.SelectedIndex = 0;
        }

        private void RefreshData()
        {
            dataGridView1.Rows.Clear();

            LayerStats stats = currentStats[listBox1.SelectedIndex];
            foreach (var i in WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows)
            {
                dataGridView1.Rows.Add(String.Format("{0}({1}+{2})", GetButtonString(i), Utility.Language["NormalMark"], Utility.Language["LongMark"]),
                    String.Format("{0}({1}+{2})", stats.NormalCounts[i] + stats.LongCounts[i], stats.NormalCounts[i], stats.LongCounts[i]));
            }
            dataGridView1.Rows.Add(Utility.Language["NormalMark"], stats.AllNormalCounts);
            dataGridView1.Rows.Add(Utility.Language["LongMark"], stats.AllLongCounts);
            dataGridView1.Rows.Add(Utility.Language["Sum"], stats.AllCounts);
            dataGridView1.Rows.Add(Utility.Language["Difficulty(Average)"], stats.Result.Average);
            dataGridView1.Rows.Add(Utility.Language["Difficulty(Peak)"], stats.Result.Peak);
        }

        private void RefreshChart()
        {
            LayerStats stats = currentStats[listBox1.SelectedIndex];
            var strList = new List<string>();
            var dataList = new List<int>();
            var iters = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                switch (CurrentChartMode)
                {
                    case ChartMode.Normal:
                        if (stats.NormalCounts[i] > 0)
                        {
                            strList.Add(GetButtonString(i));
                            dataList.Add(stats.NormalCounts[i]);
                            iters.Add(i);
                        }
                        break;
                    case ChartMode.Long:
                        if (stats.LongCounts[i] > 0)
                        {
                            strList.Add(GetButtonString(i));
                            dataList.Add(stats.LongCounts[i]);
                            iters.Add(i);
                        }
                        break;
                    case ChartMode.Both:
                        if (stats.NormalCounts[i] + stats.LongCounts[i] > 0)
                        {
                            strList.Add(GetButtonString(i));
                            dataList.Add(stats.NormalCounts[i] + stats.LongCounts[i]);
                            iters.Add(i);
                        }
                        break;
                }
            }

            chart1.Series["Default"].Points.DataBindXY(strList, dataList);
            chart1.Series["Default"].Label = "#VALX (#PERCENT)";
            int iter = 0;
            foreach (DataPoint p in chart1.Series["Default"].Points)
            {
                p.Color = Color.FromArgb(200, colorDict[(ButtonType)iters[iter]]);
                iter++;
            }
        }

        private void RefreshDifficultyChart()
        {
            LayerStats stats = currentStats[listBox1.SelectedIndex];
            var times = new List<int>();
            var points = new List<float>();
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "MyAxisXCustomFormat";
            for (int i = (int)Math.Floor(WindowUtility.IniFileWriter.StartTime); i <= (int)Math.Ceiling(WindowUtility.IniFileWriter.EndTime); i++)
            {
                times.Add(i);
                points.Add(stats.Result.Data.ContainsKey(i) ? stats.Result.Data[i] : 0);
            }
            chart2.Series["Default"].Points.DataBindXY(times, points);
        }

        private string GetButtonString(int i)
        {
            return Utility.Language[((ButtonType)i).ToString()];
        }

        void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
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

        private void データ更新ToolStripMenuItem_Click(object sender, EventArgs e)
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
            currentStats = statsList.ToArray();

            listBox1.Items.Clear();
            foreach (LayerStats stats in currentStats)
            {
                listBox1.Items.Add(stats.Name);
            }

            listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
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

        private void chart2_FormatNumber(object sender, FormatNumberEventArgs e)
        {
            if (e.ElementType == ChartElementType.AxisLabels && e.Format == "MyAxisXCustomFormat")
            {
                e.LocalizedValue = new TimeSpan(0, 0, (int)e.Value).ToString(@"mm\:ss");
            }
        }

        void Seekmain_onmoveseek(object sender, EventArgs e)
        {
            if (Visible && chart2.Visible)
            {
                var time = (int)WindowUtility.Seekmain.Currenttime;
                var normalColor = Color.Empty;
                foreach (DataPoint p in chart2.Series["Default"].Points)
                {
                    Color newColor = normalColor;
                    if ((int)p.XValue == time)
                    {
                        newColor = Color.Red;
                    }
                    p.Color = newColor;
                }
            }
        }

        class LayerStats
        {
            int[] normalCounts;
            int[] longCounts;

            public LayerStats(string name, ScoreDifficultyMeasureResult result)
            {
                Result = result;
                Name = name;
                normalCounts = new int[10];
                longCounts = new int[10];
            }

            public string Name
            {
                get;
                private set;
            }

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
            }
        }
    }
}
