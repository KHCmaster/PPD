using PPDFramework;
using PPDFrameworkCore;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 譜面の遊んだ記録を返すノード（PPD.Song.InfoEx）のテストです。記録はテストのデータベースに書く。
    /// </summary>
    public class SongInfoNodeTests
    {
        static NodeHarness InfoEx(SongInformation song, Difficulty difficulty)
        {
            var harness = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Song.InfoExFlowSourceObject));
            harness.Items["PPDGameUtility"] = new PPDGameUtility { SongInformation = song, Difficulty = difficulty };
            return harness.Initialize();
        }

        static void Write(SongInformation song, Difficulty difficulty, int score, int[] counts, ResultEvaluateType type) =>
            ScoreReaderWriter.WriteScore(song.ID, difficulty, counts, 5, score, type, 60, song);

        [Fact]
        public void CountsPlaysAndFindsBestPerformance()
        {
            using var database = new SongDatabase();
            database.AddSong("a", "guid", 100, ("Easy", null), ("Hard", null));
            SongInformation.Update();
            var song = SongDatabase.Song("a");
            Assert.True((bool)InfoEx(song, Difficulty.Easy).Get("IsAllFirstPlay"));

            // 良い判定の割合が高いものが一番（点数が高くても割合が低いものは負ける）
            Write(song, Difficulty.Easy, 1000, [8, 2, 0, 0, 0], ResultEvaluateType.Great);
            Write(song, Difficulty.Easy, 1200, [9, 0, 1, 0, 0], ResultEvaluateType.Great);
            Write(song, Difficulty.Easy, 900, [7, 3, 0, 0, 0], ResultEvaluateType.Great);
            // ミステイクはクリアに数えない
            Write(song, Difficulty.Easy, 5000, [10, 0, 0, 0, 0], ResultEvaluateType.Mistake);
            Write(song, Difficulty.Hard, 300, [1, 0, 0, 0, 0], ResultEvaluateType.Great);

            var info = InfoEx(song, Difficulty.Easy);

            Assert.Equal((4, 3, 5, 4), ((int)info.Get("PlayCount"), (int)info.Get("ClearCount"), (int)info.Get("AllPlayCount"), (int)info.Get("AllClearCount")));
            Assert.Equal(1200, info.Get("BestScore"));
            Assert.False((bool)info.Get("IsFirstPlay") || (bool)info.Get("IsAllFirstPlay"));
            Assert.Equal(1f, info.Get("BestPerformance"));
            // 割合が同じなら、点数の高いもの
            Assert.Equal((1000, 8, 2, 0, 0, 0), ((int)info.Get("BestPerformanceScore"), (int)info.Get("BestPerformanceCoolCount"), (int)info.Get("BestPerformanceGoodCount"),
                (int)info.Get("BestPerformanceSafeCount"), (int)info.Get("BestPerformanceSadCount"), (int)info.Get("BestPerformanceWorstCount")));

            var hard = InfoEx(song, Difficulty.Hard);
            Assert.Equal((1, 1), ((int)hard.Get("PlayCount"), (int)hard.Get("ClearCount")));
            Assert.True((bool)InfoEx(song, Difficulty.Normal).Get("IsFirstPlay"));
        }

        [Fact]
        public void ResultsWithoutMarks_AreCompared()
        {
            // マークのない譜面の記録（判定が 0 個）が 2 つあっても比べられる
            using var database = new SongDatabase();
            database.AddSong("a", "guid", 100, ("Easy", null));
            SongInformation.Update();
            var song = SongDatabase.Song("a");
            Write(song, Difficulty.Easy, 0, [0, 0, 0, 0, 0], ResultEvaluateType.Great);
            Write(song, Difficulty.Easy, 10, [0, 0, 0, 0, 0], ResultEvaluateType.Great);

            var info = InfoEx(song, Difficulty.Easy);

            Assert.Equal(0f, info.Get("BestPerformance"));
            Assert.Equal(10, info.Get("BestPerformanceScore"));
        }
    }
}
