using PPDEditor;
using PPDEditor.Models;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 譜面のファイルを作ります。
    /// </summary>
    static class ChartFiles
    {
        /// <summary>
        /// ppd と evd をまとめたパック（プロジェクトの 難易度.ppd）を書きます。
        /// </summary>
        public static void WritePack(string path, MarkData[] marks, IEVDData[] events)
        {
            using var writer = new PackWriter(path);
            foreach (var stream in writer.Write(["ppd", "evd"]))
            {
                if (stream.Filename == "ppd")
                {
                    PPDWriter.Write(stream, marks);
                }
                else
                {
                    EVDWriter.Write(stream, events);
                }
            }
        }

        public static MarkData Mark(float time, ButtonType button)
        {
            return new MarkData(100, 100, 0, time, button, 0);
        }
    }

    public class ChartFileReaderTests
    {
        [Fact]
        public void ReadSoundChanges_GroupsByTime()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("Base.scd");
            SCDWriter.Write(path, [new SCDData(1, ButtonType.Square, 2), new SCDData(1, ButtonType.Cross, 3), new SCDData(2, ButtonType.Square, 0)]);

            var (times, changes) = ChartFileReader.ReadSoundChanges(path).Value;

            // 最初の切り替えが 0 秒より後なら、0 秒に何も鳴らさない割り当てを入れる
            Assert.Equal([0f, 1f, 2f], times);
            Assert.All(changes[0], c => Assert.Equal(0, c));
            Assert.Equal([2, 3], changes[1].Take(2));
            Assert.Equal([0, 3], changes[2].Take(2));
        }

        [Fact]
        public void ReadSoundChanges_Empty_ReturnsNull()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("Base.scd");
            SCDWriter.Write(path, []);

            Assert.Null(ChartFileReader.ReadSoundChanges(path));
        }

        [Fact]
        public void ReadEvents_AccumulatesStatePerTime()
        {
            var stream = new MemoryStream();
            EVDWriter.Write(stream, [
                new ChangeVolumeEvent(0, 50, 0),
                new ChangeBPMEvent(0, 150),
                new RapidChangeBPMEvent(1, 200, true),
                new ChangeSoundPlayModeEvent(1, true, 1),
                new ChangeDisplayStateEvent(2, DisplayState.Hidden),
                new ChangeMoveStateEvent(2, MoveState.Stop),
                new ChangeReleaseSoundEvent(2, true, 2),
                new ChangeNoteTypeEvent(3, NoteType.AC),
                new ChangeSlideScaleEvent(3, 2),
                new ChangeInitializeOrderEvent(3, [.. Enumerable.Range(0, 10).Reverse().Select(i => (ButtonType)i)]),
            ]);
            stream.Position = 0;

            var events = ChartFileReader.ReadEvents(stream, 120);

            Assert.Equal([0f, 1f, 2f, 3f], events.Keys);
            Assert.Equal((50, 150f, false), (events[0].MovieVolumePercent, events[0].BPM, events[0].BPMRapidChange));
            Assert.Equal((200f, true, true), (events[1].BPM, events[1].BPMRapidChange, events[1].SquareKeepPlaying));
            // 前の時間の状態を引き継ぐ
            Assert.Equal(50, events[2].MovieVolumePercent);
            Assert.Equal((DisplayState.Hidden, MoveState.Stop, true), (events[2].DisplayState, events[2].MoveState, events[2].CrossReleaseSound));
            Assert.Equal((NoteType.AC, 2f), (events[3].NoteType, events[3].SlideScale));
            Assert.Equal(ButtonType.Square, events[3].InitializeOrder[9]);
            Assert.Equal(ButtonType.Square, events[2].InitializeOrder[0]);
        }

        [Fact]
        public void ReadEvents_Empty_ReturnsEmpty()
        {
            var stream = new MemoryStream();
            EVDWriter.Write(stream, []);
            stream.Position = 0;

            Assert.Empty(ChartFileReader.ReadEvents(stream, 120));
        }

        [Fact]
        public void ReadEventsFromFile_ReadsPackOrPlainFile()
        {
            using var folder = new TempFolder();
            ChartFiles.WritePack(folder.Combine("Base.ppd"), [], [new ChangeBPMEvent(1, 140)]);
            EVDWriter.Write(folder.Combine("Base.evd"), [new ChangeBPMEvent(2, 160)]);

            Assert.Equal(140, ChartFileReader.ReadEventsFromFile(folder.Combine("Base.ppd"), 100)[1].BPM);
            Assert.Equal(160, ChartFileReader.ReadEventsFromFile(folder.Combine("Base.evd"), 100)[2].BPM);
        }

        [Fact]
        public void ReadDataIni_ParsesValues()
        {
            using var folder = new TempFolder();
            folder.Write("data.ini", "[thumbtimestart]1.5\n[thumbtimeend]10\n[start]0.25\n[end]120\n[bpm]145.5\n[bpmstring]145\n"
                + "[difficulty easy]3\n[difficulty normal]5\n[difficulty hard]7\n[difficulty extreme]9\n"
                + "[moviecutleft]1\n[moviecutright]2\n[moviecuttop]3\n[moviecutbottom]4\n[guid]abc\n");

            var data = ChartFileReader.ReadDataIni(folder.Combine("data.ini"));

            Assert.Equal([1.5f, 10f, 0.25f, 120f], data.Times);
            Assert.Equal(["3", "5", "7", "9"], data.Difficulties);
            Assert.Equal((145.5f, "145", "abc"), (data.Bpm, data.BpmString, data.Guid));
            Assert.Equal((1f, 2f, 3f, 4f), (data.Trimming.Left, data.Trimming.Right, data.Trimming.Top, data.Trimming.Bottom));
        }

        [Fact]
        public void ReadDataIni_MissingValues_AreZero()
        {
            using var folder = new TempFolder();
            folder.Write("data.ini", "[bpm]abc\n");

            var data = ChartFileReader.ReadDataIni(folder.Combine("data.ini"));

            Assert.Equal(0, data.Bpm);
            Assert.All(data.Times, t => Assert.Equal(0, t));
        }

        [Theory]
        [InlineData("a.wav\r\nb.wav\r\n", new[] { "a.wav", "b.wav" })]
        [InlineData("a.wav\nb.wav", new[] { "a.wav", "b.wav" })]
        [InlineData("a.wav\rb.wav\r", new[] { "a.wav", "b.wav" })]
        [InlineData("", new string[0])]
        public void ReadSoundSet_MakesPathsInSoundFolder(string text, string[] names)
        {
            using var folder = new TempFolder();
            folder.Write("soundset.txt", text);

            var paths = ChartFileReader.ReadSoundSet(folder.Combine("soundset.txt"));

            Assert.Equal(names.Select(n => folder.Combine("sound", n)), paths);
        }
    }

    /// <summary>
    /// 発行する前の確かめのテストです。メッセージは言語ファイルの文字を使う。
    /// </summary>
    public class PublishCheckerTests : System.IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);

        public PublishCheckerTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        [Fact]
        public void CheckSameScore_FindsIdenticalCharts()
        {
            folder.Write("Easy.ppd", "same");
            folder.Write("Hard.ppd", "same");
            folder.Write("Extreme.ppd", "different");

            var errors = PublishChecker.CheckSameScore(folder.Path, AvailableDifficulty.Easy | AvailableDifficulty.Hard | AvailableDifficulty.Extreme);

            Assert.Equal(["EasyとHardの譜面は同一の可能性があり発行できません。イベント、スクリプトの設定を忘れていないか確認してください。"], errors);
            Assert.Empty(PublishChecker.CheckSameScore(folder.Path, AvailableDifficulty.Easy | AvailableDifficulty.Extreme));
        }

        [Fact]
        public void CheckNearTime_FindsSameButtonWithinOneFrame()
        {
            ChartFiles.WritePack(folder.Combine("Normal.ppd"), [
                ChartFiles.Mark(1, ButtonType.Square),
                ChartFiles.Mark(1.01f, ButtonType.Square),
                ChartFiles.Mark(1.01f, ButtonType.Cross),
                ChartFiles.Mark(2, ButtonType.Cross),
            ], []);
            using (var writer = new PackWriter(folder.Combine("Hard.ppd")))
            {
                // 譜面のない難易度は飛ばす
                foreach (var stream in writer.Write(["evd"]))
                {
                    EVDWriter.Write(stream, []);
                }
            }

            var errors = PublishChecker.CheckNearTime(folder.Path, AvailableDifficulty.Normal | AvailableDifficulty.Hard);

            var error = Assert.Single(errors);
            Assert.StartsWith("短い時間内に同じボタンのマークが複数あります。", error);
            Assert.Contains("難易度:Normal", error);
            Assert.Contains("スクエア", error);
        }
    }

    public class LayerStatsTests
    {
        [Fact]
        public void Sum_AddsLayerCounts()
        {
            var first = new LayerStats("a", null);
            first.NormalCounts[0] = 2;
            first.LongCounts[1] = 1;
            var second = new LayerStats("b", null);
            second.NormalCounts[0] = 3;
            second.NormalCounts[2] = 1;

            var sum = LayerStats.Sum("all", [ChartFiles.Mark(1, ButtonType.Square), ChartFiles.Mark(2, ButtonType.Cross)], [first, second]);

            Assert.Equal("all", sum.ToString());
            Assert.Equal(5, sum.NormalCounts[0]);
            Assert.Equal((6, 1, 7), (sum.AllNormalCounts, sum.AllLongCounts, sum.AllCounts));
            Assert.NotNull(sum.Result);
        }
    }
}
