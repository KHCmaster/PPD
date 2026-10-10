using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Docks;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditor
{
    /// <summary>
    /// 譜面の統計を表示します。処理は <see cref="StatsManagerViewModel"/> にあります。
    /// </summary>
    public sealed partial class StatsManager : DockView, IStatsSource
    {
        public StatsManager()
        {
            InitializeComponent();
            AttachViewModel(new StatsManagerViewModel(this));
        }

        public StatsManagerViewModel ViewModel
        {
            get { return (StatsManagerViewModel)DataContext; }
        }

        public void SetEvent()
        {
            // 今の時間を難易度のグラフで赤くする
            WindowUtility.Seekmain.Seeked += (sender, e) => ViewModel.HighlightTime = (int)WindowUtility.Seekmain.Currenttime;
        }

        IEnumerable<(string Name, PPDSheet Sheet)> IStatsSource.GetLayers()
        {
            return WindowUtility.LayerManager.AllLayerDisplay.Select(layer => (layer.DisplayName, layer.PPDData));
        }

        MarkData[] IStatsSource.GetAllData()
        {
            return WindowUtility.LayerManager.GetAllData(true);
        }

        IEnumerable<int> IStatsSource.VisibleRows
        {
            get { return WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows; }
        }

        (float Start, float End) IStatsSource.TimeRange
        {
            get { return (WindowUtility.IniFileWriter.StartTime, WindowUtility.IniFileWriter.EndTime); }
        }
    }
}
