using PPDCoreModel;
using PPDCoreModel.Data;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ゲームが Items で渡すもの（SoundManager、EventManager、ScoreStorage）を使う PPD のノードのテストです。
    /// </summary>
    public class GameServiceNodeTests
    {
        static NodeHarness Node(string name, IDictionary<string, object> items, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType("FlowScriptEnginePPD.FlowSourceObjects." + name, true);
            var harness = new NodeHarness(type);
            foreach (var (key, value) in items)
            {
                harness.Items[key] = value;
            }
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness.Initialize();
        }

        sealed class FakeSoundManager : ISoundManager
        {
            public List<string> Calls { get; } = [];
            public bool Result { get; set; } = true;
            public bool Play(MarkType button, double playRatio) => Add($"play {button} ratio={playRatio}");
            public bool Play(MarkType button, int volume, double playRatio) => Add($"play {button} volume={volume} ratio={playRatio}");
            public bool Play(int index, int volume, double playRatio) => Add($"play #{index} volume={volume} ratio={playRatio}");
            public bool Stop(MarkType button) => Add($"stop {button}");
            public bool Stop(int index) => Add($"stop #{index}");

            bool Add(string call)
            {
                Calls.Add(call);
                return Result;
            }
        }

        [Fact]
        public void SoundManager_PassesButtonVolumeAndPlayRatio()
        {
            var sound = new FakeSoundManager();
            var items = new Dictionary<string, object> { ["SoundManager"] = sound, ["PlayRatio"] = 1.5 };
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node("SoundManager." + name, items, inputs).Invoke().Fired);

            Ok("PlayByButtonFlowSourceObject", ("Button", MarkType.Circle));
            Ok("PlayByButtonWithVolumeFlowSourceObject", ("Button", MarkType.Up), ("Volume", -500));
            Ok("PlayFlowSourceObject", ("Index", 3), ("Volume", -200));
            Ok("StopByButtonFlowSourceObject", ("Button", MarkType.L));
            Ok("StopFlowSourceObjectBase", ("Index", 4));

            Assert.Equal([
                "play Circle ratio=1.5",
                "play Up volume=-500 ratio=1.5",
                "play #3 volume=-200 ratio=1.5",
                "stop L",
                "stop #4",
            ], sound.Calls);
        }

        [Fact]
        public void SoundManager_VolumeIsClampedToRange()
        {
            var sound = new FakeSoundManager();
            var items = new Dictionary<string, object> { ["SoundManager"] = sound };

            Node("SoundManager.PlayFlowSourceObject", items, ("Index", 0), ("Volume", 100)).Invoke();
            Node("SoundManager.PlayFlowSourceObject", items, ("Index", 0), ("Volume", -20000)).Invoke();
            Node("SoundManager.PlayByButtonWithVolumeFlowSourceObject", items, ("Button", MarkType.Square), ("Volume", -20000)).Invoke();

            // 音量は -10000（無音）から 0（最大）。PlayRatio がなければ 0
            Assert.Equal(["play #0 volume=0 ratio=0", "play #0 volume=-10000 ratio=0", "play Square volume=-10000 ratio=0"], sound.Calls);
        }

        [Theory]
        [InlineData("PlayByButtonFlowSourceObject")]
        [InlineData("PlayByButtonWithVolumeFlowSourceObject")]
        [InlineData("PlayFlowSourceObject")]
        [InlineData("StopByButtonFlowSourceObject")]
        [InlineData("StopFlowSourceObjectBase")]
        public void SoundManager_FailsWhenRefusedOrOutsideGame(string name)
        {
            var refusing = new Dictionary<string, object> { ["SoundManager"] = new FakeSoundManager { Result = false } };

            Assert.Equal(["Failed"], Node("SoundManager." + name, refusing).Invoke().Fired);
            Assert.Equal(["Failed"], Node("SoundManager." + name, new Dictionary<string, object>()).Invoke().Fired);
        }

        sealed class FakeEventManager : IEventManager
        {
            public int[] VolumePercents { get; } = new int[10];
            public bool[] KeepPlayings { get; } = new bool[10];
            public bool[] ReleaseSounds { get; } = new bool[10];
            public bool SetVolumePercent(MarkType button, int volPercent) => Set(VolumePercents, button, volPercent);
            public bool SetKeepPlaying(MarkType button, bool keepPlaying) => Set(KeepPlayings, button, keepPlaying);
            public bool SetReleaseSound(MarkType button, bool releaseSound) => Set(ReleaseSounds, button, releaseSound);

            static bool Set<T>(T[] values, MarkType button, T value)
            {
                if (button < 0 || (int)button >= values.Length)
                {
                    return false;
                }
                values[(int)button] = value;
                return true;
            }
        }

        [Fact]
        public void EventManager_SetsAndReadsButtonSettings()
        {
            var events = new FakeEventManager();
            var items = new Dictionary<string, object> { ["EventManager"] = events };
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node("EventManager." + name, items, inputs).Invoke().Fired);

            Ok("SetVolumePercentFlowSourceObject", ("Button", MarkType.Cross), ("VolumePercent", 80));
            Ok("SetKeepPlayingFlowSourceObject", ("Button", MarkType.Right), ("KeepPlaying", true));
            Ok("SetReleaseSoundFlowSourceObject", ("Button", MarkType.R), ("ReleaseSound", true));

            var value = Node("EventManager.ValueFlowSourceManager", items);
            Assert.Equal(80, ((object[])value.Get("VolumePercents"))[(int)MarkType.Cross]);
            Assert.Equal(true, ((object[])value.Get("KeepPlayings"))[(int)MarkType.Right]);
            Assert.Equal(true, ((object[])value.Get("ReleaseSounds"))[(int)MarkType.R]);

            // ゲームが受け付けないボタン
            Assert.Equal(["Failed"], Node("EventManager.SetVolumePercentFlowSourceObject", items, ("Button", (MarkType)20)).Invoke().Fired);
        }

        [Fact]
        public void EventManager_OutsideGame()
        {
            var none = new Dictionary<string, object>();
            foreach (var name in new[] { "SetVolumePercentFlowSourceObject", "SetKeepPlayingFlowSourceObject", "SetReleaseSoundFlowSourceObject" })
            {
                Assert.Equal(["Failed"], Node("EventManager." + name, none).Invoke().Fired);
            }
            var value = Node("EventManager.ValueFlowSourceManager", none);
            Assert.Null(value.Get("VolumePercents"));
            Assert.Null(value.Get("KeepPlayings"));
            Assert.Null(value.Get("ReleaseSounds"));
        }

        [Fact]
        public void ScoreStorage_SetGetAndContains()
        {
            var storage = new PPDFramework.ScoreStorage(1);
            var items = new Dictionary<string, object> { ["ScoreStorage"] = storage };

            Node("ScoreStorage.SetFlowSourceObject", items, ("Key", "best"), ("Value", "100")).Invoke();

            Assert.Equal("100", storage["best"]);
            Assert.Equal("100", Node("ScoreStorage.GetFlowSourceObject", items, ("Key", "best")).Get("Value"));
            Assert.Equal(true, Node("ScoreStorage.ContainsFlowSourceObject", items, ("Key", "best")).Get("Value"));
            Assert.Equal(false, Node("ScoreStorage.ContainsFlowSourceObject", items, ("Key", "other")).Get("Value"));
            Assert.Null(Node("ScoreStorage.GetFlowSourceObject", items, ("Key", "other")).Get("Value"));
        }

        [Fact]
        public void ScoreStorage_OutsideGame_DoesNothing()
        {
            var none = new Dictionary<string, object>();

            Node("ScoreStorage.SetFlowSourceObject", none, ("Key", "best"), ("Value", "100")).Invoke();

            Assert.Null(Node("ScoreStorage.GetFlowSourceObject", none, ("Key", "best")).Get("Value"));
            Assert.Equal(false, Node("ScoreStorage.ContainsFlowSourceObject", none, ("Key", "best")).Get("Value"));
        }
    }
}
