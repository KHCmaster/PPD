using PPDCore;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 譜面のイベント（BPM、音量、表示、止まる区間など）を時間に合わせて反映する、ゲームの EventManager のテストです。
    /// </summary>
    public class GameEventManagerTests
    {
        static readonly ButtonType[] Reversed = [.. Enumerable.Range(0, 10).Reverse().Select(i => (ButtonType)i)];

        (EventManager Events, FlowScriptManager Script, List<int> MovieVolumes) Create(MainGameConfigBase config = null, params IEVDData[] events)
        {
            var song = new SongInformation();
            typeof(SongInformation).GetProperty(nameof(SongInformation.BPM)).SetValue(song, 120f);
            var utility = new PPDGameUtility { SongInformation = song };
            var stream = new MemoryStream();
            EVDWriter.Write(stream, events);
            stream.Position = 0;
            var script = Script();
            var manager = new EventManager(utility, stream, config ?? MainGameConfigBase.Default, script);
            var volumes = new List<int>();
            manager.ChangeMovieVolume += volumes.Add;
            return (manager, script, volumes);
        }

        // EventManager はスクリプトの BPMManager だけを使う（コンストラクターは dlls のスクリプトを読むので使わない）
        static FlowScriptManager Script()
        {
            var script = (FlowScriptManager)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(FlowScriptManager));
            typeof(FlowScriptManager).GetProperty(nameof(FlowScriptManager.BPMManager)).SetValue(script, new PPDCoreModel.BPMManager());
            return script;
        }

        static IEVDData[] Events() =>
        [
            new ChangeVolumeEvent(0, 50, 0),
            new ChangeVolumeEvent(0, 70, 2),
            new ChangeBPMEvent(1, 150),
            new ChangeSoundPlayModeEvent(1, true, 1),
            new ChangeReleaseSoundEvent(1, true, 3),
            new RapidChangeBPMEvent(2, 200, true),
            new ChangeDisplayStateEvent(3, DisplayState.Hidden),
            new ChangeNoteTypeEvent(3, NoteType.AC),
            new ChangeSlideScaleEvent(3, 2),
            new ChangeInitializeOrderEvent(3, Reversed),
            new ChangeMoveStateEvent(4, MoveState.Stop),
            new ChangeMoveStateEvent(5, MoveState.Normal),
        ];

        [Fact]
        public void Update_AppliesEventsAsTimePasses()
        {
            var (events, script, volumes) = Create(null, Events());
            Assert.Equal(120f, events.BPM);

            events.Update(0);
            // 映像の音量は -100 * (100 - 音量)
            Assert.Equal([-5000], volumes);
            Assert.Equal(70, events.GetVolPercent(1));
            Assert.Equal(90, events.GetVolPercent(0));

            events.Update(1);
            Assert.Equal(150f, script.BPMManager.TargetBPM);
            Assert.True(events.KeepPlayings[0]);
            Assert.True(events.GetReleaseSound(2));
            // BPM は 1 ずつ近づく
            Assert.Equal(121f, events.BPM);

            events.Update(2);
            Assert.Equal(200f, events.BPM);
            Assert.Equal(0, events.GetVolPercent(10));
            Assert.False(events.GetReleaseSound(-1));
        }

        [Fact]
        public void StateAtMarkTime()
        {
            var (events, _, _) = Create(null, Events());

            Assert.Equal(DisplayState.Normal, events.GetCorrectDisplaystate(2.9f));
            Assert.Equal(DisplayState.Hidden, events.GetCorrectDisplaystate(3));
            Assert.Equal(NoteType.Normal, events.GetNoteType(1));
            Assert.Equal(NoteType.AC, events.GetNoteType(10));
            Assert.Equal(1f, events.GetSlideScale(0));
            Assert.Equal(2f, events.GetSlideScale(3));
            Assert.Equal(ButtonType.Square, events.GetInitlaizeOrder(0)[0]);
            Assert.Equal(Reversed, events.GetInitlaizeOrder(4));
        }

        sealed class SuddenConfig : MainGameConfigBase
        {
            public override DisplayState DisplayState => DisplayState.Sudden;
        }

        [Fact]
        public void ConfigDisplayState_WinsOverEvents()
        {
            var (events, _, _) = Create(new SuddenConfig(), Events());

            Assert.Equal(DisplayState.Sudden, events.GetCorrectDisplaystate(0));
            Assert.Equal(DisplayState.Sudden, events.GetCorrectDisplaystate(10));
        }

        [Theory]
        // 止まる区間（4 から 5 秒）の前のマークには関係ない
        [InlineData(3.5f, 4.5f, 3.5f)]
        // 止まる区間の後のマークは、区間の長さだけ遅れて見える
        [InlineData(3.5f, 6, 4.5f)]
        // 止まっている間は区間の終わりで止まって見える
        [InlineData(4.5f, 6, 5)]
        [InlineData(5.5f, 6, 5.5f)]
        public void StopRanges_DelayLaterMarks(float current, float markTime, float expected)
        {
            var (events, _, _) = Create(null, Events());

            Assert.Equal(expected, events.GetCorrectTime(current, markTime));
        }

        [Fact]
        public void SeekAndRetry()
        {
            var (events, script, volumes) = Create(null, Events());

            events.Seek(2.5f);
            Assert.Equal(200f, script.BPMManager.CurrentBPM);
            Assert.True(events.KeepPlayings[0]);
            Assert.Equal([-1000, -5000], volumes);

            events.Retry();
            Assert.Equal(120f, script.BPMManager.CurrentBPM);
            Assert.False(events.KeepPlayings[0]);
            Assert.All(events.VolumePercents, v => Assert.Equal(90, v));
            Assert.All(events.ReleaseSounds, Assert.False);
        }

        [Fact]
        public void ScriptSetters()
        {
            var (events, _, _) = Create(null, Events());

            Assert.True(events.SetVolumePercent(MarkType.L, 10));
            Assert.True(events.SetKeepPlaying(MarkType.Up, true));
            Assert.True(events.SetReleaseSound(MarkType.Square, true));
            Assert.False(events.SetVolumePercent((MarkType)20, 10));

            Assert.Equal(10, events.GetVolPercent(9));
            Assert.True(events.KeepPlayings[7]);
            Assert.True(events.GetReleaseSound(0));
        }

        [Fact]
        public void BadEventData_IsIgnored()
        {
            var song = new SongInformation();
            var script = Script();
            var events = new EventManager(new PPDGameUtility { SongInformation = song }, new MemoryStream([1, 2, 3]), MainGameConfigBase.Default, script);

            events.Update(10);
            events.Seek(10);

            Assert.Equal(DisplayState.Normal, events.GetCorrectDisplaystate(10));
            Assert.Equal(90, events.GetVolPercent(0));
        }
    }
}
