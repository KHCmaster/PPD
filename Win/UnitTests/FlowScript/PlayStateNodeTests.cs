using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// 曲を遊んでいる間の状態を使う PPD のノード（BPM、入力、ホールド、歌詞、曲の情報）のテストです。
    /// </summary>
    public class PlayStateNodeTests
    {
        static NodeHarness Node(string name, Dictionary<string, object> items = null, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            foreach (var (key, value) in items ?? [])
            {
                harness.Items[key] = value;
            }
            // スクリプトと同じく、初期化してから値を入れる
            harness.Initialize();
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        [Fact]
        public void BPMManager_StepsTowardTarget()
        {
            var bpm = new BPMManager { CurrentBPM = 120, TargetBPM = 122.5f };

            bpm.Step();
            Assert.Equal(121, bpm.CurrentBPM);
            // 1 ずつ近づき、1 より近くなったら目標にそろえる
            bpm.Step();
            Assert.Equal(122.5f, bpm.CurrentBPM);
            bpm.Step();
            Assert.Equal(122.5f, bpm.CurrentBPM);

            bpm.TargetBPM = 120;
            bpm.Step();
            Assert.Equal(121.5f, bpm.CurrentBPM);
        }

        [Fact]
        public void BPM_SetsCurrentAndTarget()
        {
            var manager = new BPMManager { CurrentBPM = 120, TargetBPM = 120 };
            var items = new Dictionary<string, object> { ["BPMManager"] = manager };

            // 今の BPM を変えると、目標も同じにする（すぐ変わる）
            Node("BPM", items, ("CurrentBPM", 150f)).Invoke("SetCurrentBPM");
            Assert.Equal((150f, 150f), (manager.CurrentBPM, manager.TargetBPM));

            // 目標だけ変えると、少しずつ近づく
            Node("BPM", items, ("TargetBPM", 180f)).Invoke("SetTargetBPM");
            Assert.Equal((150f, 180f), (manager.CurrentBPM, manager.TargetBPM));

            var value = Node("BPM", items);
            Assert.Equal(150f, value.Get("CurrentBPM"));
            Assert.Equal(180f, value.Get("TargetBPM"));
        }

        [Fact]
        public void BPM_OutsideGame_IsZero()
        {
            var bpm = Node("BPM", null, ("CurrentBPM", 150f), ("TargetBPM", 180f));

            Assert.Equal(0f, bpm.Get("CurrentBPM"));
            Assert.Equal(0f, bpm.Get("TargetBPM"));
        }

        [Fact]
        public void InputManager_DisablesButtons()
        {
            var input = new InputManager();
            Assert.False(input.Modified);

            input.Disable(MarkType.Circle, false);
            Assert.False(input.Modified);
            input.Disable(MarkType.Circle, true);
            Assert.True(input.Modified);
            Assert.True(input.IsDisabled(MarkType.Circle));
            Assert.False(input.IsDisabled(MarkType.Cross));

            // ForceEnable の間はどれも使える
            input.ForceEnable();
            Assert.False(input.IsDisabled(MarkType.Circle));

            input.Initialize();
            Assert.False(input.Modified);
            Assert.False(input.IsDisabled(MarkType.Circle));
        }

        [Fact]
        public void DisableInput_Node()
        {
            var input = new InputManager();
            var items = new Dictionary<string, object> { ["InputManager"] = input };

            Assert.Equal(["Success"], Node("DisableInput", items, ("Button", MarkType.R), ("IsDisabled", true)).Invoke().Fired);
            Assert.True(input.IsDisabled(MarkType.R));
            Assert.Equal(["Failed"], Node("DisableInput").Invoke().Fired);
        }

        [Fact]
        public void Input_FiresForChosenButtonOrAny()
        {
            var circle = Node("Input", null, ("Button", MarkType.Circle));
            var any = Node("Input", null, ("HandleAny", true));
            static void Press(NodeHarness h, MarkType button) => h.Node.Manager.Call("PPD.Input", [button, 3, true, false]);

            Press(circle, MarkType.Cross);
            Assert.Empty(circle.Fired);
            Press(circle, MarkType.Circle);
            Assert.Equal(["Out"], circle.Fired);
            Assert.Equal(3, circle.Get("PressCount"));
            Assert.Equal(true, circle.Get("Pressed"));
            Assert.Equal(false, circle.Get("Released"));

            Press(any, MarkType.Up);
            Assert.Equal(["Out"], any.Fired);
            Assert.Equal(MarkType.Up, any.Get("Button"));
        }

        [Fact]
        public void Hold_FiresForEachStage()
        {
            var hold = Node("Hold");
            var stages = new[] { (HoldInfo.HoldStart, "Start"), (HoldInfo.HoldChange, "Change"), (HoldInfo.MaxHold, "MaxHold"), (HoldInfo.HoldEnd, "End") };

            var score = 0;
            foreach (var (callBack, _) in stages)
            {
                score += 100;
                hold.Node.Manager.Call(callBack, [new HoldInfo(score, 5000, [MarkType.Circle, MarkType.Cross])]);
            }

            Assert.Equal(stages.Select(s => s.Item2), hold.Fired);
            Assert.Equal(400, hold.Get("CurrentHoldScore"));
            Assert.Equal(5000, hold.Get("MaxHoldBonus"));
            Assert.Equal(new object[] { MarkType.Circle, MarkType.Cross }, [.. ((IEnumerable<object>)hold.Get("HoldMarks"))]);
        }

        sealed class FakeLyrics : ILylics
        {
            public string Lylics => "歌詞";
        }

        [Fact]
        public void Lyrics()
        {
            Assert.Equal("歌詞", Node("Song.Lyrics", new() { ["Lylics"] = new FakeLyrics() }).Get("Value"));
            Assert.Equal("", Node("Song.Lyrics").Get("Value"));
        }

        sealed class FakeMarkManager : IMarkManager
        {
            public void AddMark(Vector2 position, float angle, float time, MarkType markType, int id) { }
            public void AddLongMark(Vector2 position, float angle, float time, float endTime, MarkType markType, int id) { }
            public int AllMarkCount => 30;
            public int AllLongMarkCount => 4;
            public ScoreDifficultyMeasureResult ScoreDifficultyMeasureResult { get; } = (ScoreDifficultyMeasureResult)typeof(ScoreDifficultyMeasureResult)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0].Invoke([new SortedDictionary<int, float> { [0] = 3, [1] = 1 }]);
            public MarkDataBase[] Marks { get; } = [new MarkData(0, 0, 0, 1, ButtonType.Circle, 0)];
            public int GetMarkCount(int sameNum) => 10 + sameNum;
        }

        [Fact]
        public void SongInfo_ReadsGameSettingsAndMarks()
        {
            var utility = new PPDGameUtility
            {
                AutoMode = AutoMode.All,
                Connect = true,
                Difficulty = PPDFrameworkCore.Difficulty.Hard,
                DifficultString = "HARD 8",
                GodMode = true,
                IsDebug = true,
                MuteSE = true,
                Random = true,
                SpeedScale = 1.5f,
            };
            var info = Node("Song.Info", new() { ["PPDGameUtility"] = utility, ["MarkManager"] = new FakeMarkManager() });

            foreach (var flag in new[] { "IsAuto", "IsConnect", "IsGodMode", "IsDebug", "IsMuteSE", "IsRandom" })
            {
                Assert.Equal(true, info.Get(flag));
            }
            // GodMode などを使うと、記録に残る遊び方ではない
            Assert.Equal(false, info.Get("IsRegular"));
            Assert.Equal("HARD 8", info.Get("DifficultyString"));
            Assert.Equal(PPDFrameworkCore.Difficulty.Hard, info.Get("Difficulty"));
            Assert.Equal(1.5f, info.Get("SpeedScale"));
            Assert.Equal(30, info.Get("AllMarkCount"));
            Assert.Equal(4, info.Get("AllLongMarkCount"));
            Assert.Equal([10, 11, 12, 13, 14, 15], new[] { "One", "Two", "Three", "Four", "Five", "Six" }.Select(n => info.Get(n + "MarkCount")));
            Assert.Equal(2f, info.Get("DifficultyPointAverage"));
            Assert.Equal(3f, info.Get("DifficultyPointPeak"));
            Assert.Single((object[])info.Get("Marks"));
        }

        [Fact]
        public void SongInfo_OutsideGame_IsDefault()
        {
            var info = Node("Song.Info");

            Assert.Equal(false, info.Get("IsAuto"));
            Assert.Equal("", info.Get("DifficultyString"));
            Assert.Equal(PPDFrameworkCore.Difficulty.Other, info.Get("Difficulty"));
            Assert.Equal(1f, info.Get("SpeedScale"));
            Assert.Equal("", info.Get("AuthorName"));
            Assert.Equal(0f, info.Get("BPM"));
            Assert.Equal(0f, info.Get("StartTime"));
            Assert.Equal(0f, info.Get("EndTime"));
            Assert.Equal("", info.Get("SongName"));
            Assert.Equal(0, info.Get("AllMarkCount"));
            Assert.Equal(0, info.Get("SixMarkCount"));
            Assert.Equal(0f, info.Get("DifficultyPointAverage"));
            Assert.Equal(0f, info.Get("DifficultyPointPeak"));
            Assert.Equal(NoteType.Normal, info.Get("NoteType"));
            Assert.Null(info.Get("Marks"));
        }
    }
}
