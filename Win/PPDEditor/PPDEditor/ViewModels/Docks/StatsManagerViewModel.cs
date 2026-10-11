using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Models;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// 統計を求めるのに使う、エディターの情報です。
    /// </summary>
    public interface IStatsSource
    {
        /// <summary>
        /// レイヤーの名前と譜面です。
        /// </summary>
        IEnumerable<(string Name, PPDSheet Sheet)> GetLayers();

        /// <summary>
        /// すべてのレイヤーのマークです。
        /// </summary>
        MarkData[] GetAllData();

        /// <summary>
        /// 表示しているタイムラインの行（ボタン）の順です。
        /// </summary>
        IEnumerable<int> VisibleRows { get; }

        /// <summary>
        /// 曲の開始時間と終了時間（秒）です。
        /// </summary>
        (float Start, float End) TimeRange { get; }
    }

    /// <summary>
    /// 円グラフに含めるマークの種類です。
    /// </summary>
    public enum ChartMode
    {
        Both,
        Normal,
        Long
    }

    /// <summary>
    /// 譜面の統計（マークの数、円グラフ、時間ごとの難易度）を表示します。
    /// </summary>
    public partial class StatsManagerViewModel : DockViewModel
    {
        static readonly Dictionary<ButtonType, Color> colorDict = new()
        {
            { ButtonType.Square, Color.FromRgb(231, 75, 245) },
            { ButtonType.Cross, Color.FromRgb(100, 185, 239) },
            { ButtonType.Circle, Color.FromRgb(238, 54, 80) },
            { ButtonType.Triangle, Color.FromRgb(24, 208, 37) },
            { ButtonType.Left, Color.FromRgb(255, 65, 221) },
            { ButtonType.Down, Color.FromRgb(65, 208, 255) },
            { ButtonType.Right, Color.FromRgb(255, 65, 98) },
            { ButtonType.Up, Color.FromRgb(66, 254, 77) },
            { ButtonType.R, Color.FromRgb(181, 245, 251) },
            { ButtonType.L, Color.FromRgb(75, 231, 245) }
        };

        [ObservableProperty]
        public partial LayerStats SelectedLayer { get; set; }

        [ObservableProperty]
        public partial int ChartModeIndex { get; set; }

        [ObservableProperty]
        public partial IReadOnlyList<KeyValuePair<string, object>> DataRows { get; set; } = [];

        [ObservableProperty]
        public partial IReadOnlyList<PieChartItem> PieItems { get; set; } = [];

        [ObservableProperty]
        public partial IReadOnlyList<KeyValuePair<int, float>> DifficultyPoints { get; set; } = [];

        /// <summary>
        /// 難易度のグラフで赤くする秒（今の時間）です。
        /// </summary>
        [ObservableProperty]
        public partial int? HighlightTime { get; set; }

        readonly IStatsSource source;

        public StatsManagerViewModel(IStatsSource source)
        {
            this.source = source;
            // 言語を変えたら、種類の名前を作り直して最初の種類にする（以前と同じ）
            Texts.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(ChartModeNames));
                ChartModeIndex = 0;
            };
        }

        public ObservableCollection<LayerStats> Layers { get; } = [];

        /// <summary>
        /// 円グラフに含めるマークの種類の名前です（<see cref="ChartMode"/> の順）。
        /// </summary>
        public string[] ChartModeNames
        {
            get { return [String.Format("{0}+{1}", Texts["NormalMark"], Texts["LongMark"]), Texts["NormalMark"], Texts["LongMark"]]; }
        }

        private ChartMode CurrentChartMode
        {
            get { return (ChartMode)ChartModeIndex; }
        }

        partial void OnSelectedLayerChanged(LayerStats value)
        {
            if (value != null)
            {
                RefreshData();
                RefreshChart();
                RefreshDifficultyChart();
            }
        }

        partial void OnChartModeIndexChanged(int value)
        {
            if (SelectedLayer != null)
            {
                RefreshChart();
            }
        }

        /// <summary>
        /// レイヤーごとと、すべてのレイヤーの統計を求め直します。
        /// </summary>
        [RelayCommand]
        private void Update()
        {
            var layers = source.GetLayers().Select(layer => LayerStats.FromSheet(layer.Name, layer.Sheet)).ToList();
            Layers.Clear();
            Layers.Add(LayerStats.Sum(Texts["AllLayers"], source.GetAllData(), layers));
            foreach (var stats in layers)
            {
                Layers.Add(stats);
            }
            SelectedLayer = Layers[0];
        }

        private void RefreshData()
        {
            var stats = SelectedLayer;
            var rows = new List<KeyValuePair<string, object>>();
            foreach (var i in source.VisibleRows)
            {
                rows.Add(new(String.Format("{0}({1}+{2})", GetButtonString(i), Texts["NormalMark"], Texts["LongMark"]),
                    String.Format("{0}({1}+{2})", stats.NormalCounts[i] + stats.LongCounts[i], stats.NormalCounts[i], stats.LongCounts[i])));
            }
            rows.Add(new(Texts["NormalMark"], stats.AllNormalCounts));
            rows.Add(new(Texts["LongMark"], stats.AllLongCounts));
            rows.Add(new(Texts["Sum"], stats.AllCounts));
            rows.Add(new(Texts["Difficulty(Average)"], stats.Result.Average));
            rows.Add(new(Texts["Difficulty(Peak)"], stats.Result.Peak));
            DataRows = rows;
        }

        private void RefreshChart()
        {
            var stats = SelectedLayer;
            var items = new List<PieChartItem>();
            for (int i = 0; i < 10; i++)
            {
                var count = CurrentChartMode switch
                {
                    ChartMode.Normal => stats.NormalCounts[i],
                    ChartMode.Long => stats.LongCounts[i],
                    _ => stats.NormalCounts[i] + stats.LongCounts[i],
                };
                if (count > 0)
                {
                    var color = colorDict[(ButtonType)i];
                    items.Add(new PieChartItem(GetButtonString(i), count, Color.FromArgb(200, color.R, color.G, color.B)));
                }
            }
            PieItems = items;
        }

        // 曲の開始から終了までの、1 秒ごとの難易度
        private void RefreshDifficultyChart()
        {
            var (start, end) = source.TimeRange;
            var points = new List<KeyValuePair<int, float>>();
            for (int i = (int)Math.Floor(start); i <= (int)Math.Ceiling(end); i++)
            {
                points.Add(new(i, SelectedLayer.Result.Data.TryGetValue(i, out float value) ? value : 0));
            }
            DifficultyPoints = points;
        }

        private string GetButtonString(int i)
        {
            return Texts[((ButtonType)i).ToString()];
        }
    }
}
