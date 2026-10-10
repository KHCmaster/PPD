using FlaUI.Core.AutomationElements;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor の統計のドックで、マークの数などのデータを更新し、グラフ（割合と、時間ごとの難しさ）を出すテストです。
    /// </summary>
    public sealed class PPDEditorStatsTests() : PPDEditorProjectTestBase("PPDEditor.StatsManager")
    {
        [Fact]
        public void StatisticsAndChartsAreShown()
        {
            CreateProject();
            SetBeatWidth();
            var timeline = session.Find("Seekmain");
            foreach (var (beat, row) in new[] { (1, 0), (2, 1), (3, 2), (3, 3) })
            {
                session.DoubleClickAt(timeline, Beat(beat), Row(row));
            }

            // データを更新して、レイヤーを選ぶ
            session.FindMenuItem("StatsUpdateMenuItem").Invoke();
            var layers = session.Find("StatsLayerList").AsListBox();
            Assert.True(AppSession.WaitUntil(() => layers.Items.Length > 0), "レイヤーが出ません");
            layers.Items[0].Select();

            // データ、割合のグラフ（種類を切り替える）、難しさのグラフを順に出す
            // 表の中身が増えると、ウィンドウ全体から探すのに時間がかかるので、タブはタブの並びの中から探す
            var dataTab = session.Find("StatsDataTab").AsTabItem();
            var tabs = dataTab.Parent;
            var chartTab = tabs.FindFirstChild(cf => cf.ByAutomationId("StatsChartTab")).AsTabItem();
            var difficultyTab = tabs.FindFirstChild(cf => cf.ByAutomationId("StatsDifficultyTab")).AsTabItem();
            dataTab.Select();
            chartTab.Select();
            var mode = chartTab.FindFirstDescendant(cf => cf.ByAutomationId("StatsChartModeBox")).AsComboBox();
            foreach (var index in Enumerable.Range(0, mode.Items.Length))
            {
                mode.Select(index);
                System.Threading.Thread.Sleep(200);
            }
            difficultyTab.Select();
            System.Threading.Thread.Sleep(500);
            Assert.Empty(session.ModalWindowsOf());
        }
    }
}
