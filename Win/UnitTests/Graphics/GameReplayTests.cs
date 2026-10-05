using PPDCore;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnitTests.Libraries;
using UnitTests.PPDEditor;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// リプレイのデータと、リプレイからゲーム全体（MainGameComponent）を動かして点数の流れ（ゴースト）を作るテストです。
    /// </summary>
    /// <remarks>
    /// ゴーストを作るときは、動画、Mod、ネットワークを使わない。譜面はテストの一時フォルダーのデータベースに入れる。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class GameReplayTests(GraphicsDevice graphics)
    {
        /// <summary>
        /// どのメソッドも何もせず、既定の値を返すインターフェースの実装を作ります。
        /// </summary>
        public class Nothing : DispatchProxy
        {
            public static T Create<T>() => DispatchProxy.Create<T, Nothing>();

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                var type = targetMethod.ReturnType;
                return type == typeof(void) || !type.IsValueType ? null : Activator.CreateInstance(type);
            }
        }

        static PPDPlayRecorder Record(params (float Time, ButtonType[] Pressed, ButtonType[] Released)[] frames)
        {
            var recorder = new PPDPlayRecorder();
            foreach (var (time, pressed, released) in frames)
            {
                var count = new int[10];
                var p = new bool[10];
                var r = new bool[10];
                foreach (var button in pressed)
                {
                    p[(int)button] = true;
                    count[(int)button] = 1;
                }
                foreach (var button in released)
                {
                    r[(int)button] = true;
                }
                recorder.Update(time, (long)(time * 1000), count, p, r);
            }
            return recorder;
        }

        // 60 フレームごとに時間を進め、指定した時間にボタンを押す
        static PPDPlayRecorder Play(float end, params (float Time, ButtonType Button)[] presses)
        {
            var frames = new List<(float, ButtonType[], ButtonType[])>();
            for (var frame = 0; frame <= end * 60; frame++)
            {
                var time = frame / 60f;
                frames.Add((time, [.. presses.Where(p => Math.Abs(p.Time - time) < 1 / 120f).Select(p => p.Button)], []));
            }
            return Record([.. frames]);
        }

        [Fact]
        public void ReplayData_RoundTrip()
        {
            var recorder = Record(
                (0.5f, [ButtonType.Circle, ButtonType.R], []),
                (0.75f, [], [ButtonType.Circle]));

            var decoder = PPDPlayDecorder.FromBytes(recorder.GetBytes());

            Assert.Equal(2, decoder.Length);
            var count = new int[10];
            var pressed = new bool[10];
            var released = new bool[10];
            // まだの時間は読まない
            Assert.False(decoder.Update(0.4f, count, pressed, released, out _, out _));
            Assert.True(decoder.Update(1, count, pressed, released, out var time, out var currentTime));
            Assert.Equal((0.5f, 500L), (time, currentTime));
            Assert.Equal([ButtonType.Circle, ButtonType.R], Enumerable.Range(0, 10).Where(i => pressed[i]).Select(i => (ButtonType)i));
            Assert.Equal(1, count[(int)ButtonType.R]);
            Assert.True(decoder.Update(1, count, pressed, released, out time, out _));
            Assert.Equal(0.75f, time);
            Assert.True(released[(int)ButtonType.Circle]);
            Assert.False(pressed[(int)ButtonType.Circle]);
            Assert.True(decoder.IsEnd);
            Assert.False(decoder.Update(10, count, pressed, released, out _, out _));
            decoder.Reset();
            Assert.Equal(0, decoder.Position);

            // ほかの版のデータは読まない
            using var stream = new MemoryStream();
            using (var writer = new PPDPack.PackWriter(stream))
            {
                foreach (var s in writer.Write(["Version"]))
                {
                    s.WriteByte(1);
                }
            }
            Assert.Throws<Exception>(() => PPDPlayDecorder.FromBytes(stream.ToArray()));
        }

        GhostFrame[] Ghost(MarkData[] marks, IEVDData[] events, PPDPlayRecorder replay)
        {
            Assert.False(TcpDebugControllerBase.IsListening());
            using var database = new SongDatabase();
            Directory.CreateDirectory("dlls");
            using var resources = new GameResources(graphics, setCurrentDirectory: false);
            foreach (var effect in new[] { @"hold\holdback.etd", @"hold\maxback.etd", @"slide\max_slide_effect.etd", @"slide\hatch_right.etd", @"slide\hatch_left.etd" })
            {
                resources.WriteEffect(Path.Combine(@"img\PPD\main_game", effect));
            }
            var dir = database.AddSong("song", "guid", 130, ("Easy", null));
            ChartFiles.WritePack(Path.Combine(dir, "Easy.ppd"), marks, events);
            SongInformation.Update();
            var utility = new PPDGameUtility
            {
                SongInformation = SongDatabase.Song("song"),
                Difficulty = Difficulty.Easy,
                Profile = ProfileManager.Instance.Default,
                SpeedScale = 1,
                AppliedMods = [],
            };
            using var game = new MainGameComponent(graphics.Device, Nothing.Create<IGameHost>(), resources.Resources, Nothing.Create<ISound>(), null,
                utility, null, resources.MarkImages, null, null, MainGameConfigBase.Default, 0, 0);

            game.Initialize(play: false, isGhost: true);
            var progress = new List<int>();
            var frames = game.GetGhostFrames(replay.GetBytes(), progress.Add);

            Assert.Equal(100, progress.Last());
            return frames;
        }

        [Fact]
        public void Ghost_FollowsScoreLifeAndEvaluation()
        {
            MarkData[] marks = [ChartFiles.Mark(1, ButtonType.Circle), ChartFiles.Mark(2, ButtonType.Cross), ChartFiles.Mark(3, ButtonType.Square)];

            // 1 つ目はぴったり、2 つ目は押さない、3 つ目は少し遅れて押す
            var frames = Ghost(marks, [], Play(4, (1, ButtonType.Circle), (3.1f, ButtonType.Square)));

            var evaluations = frames.Where(f => f.MarkEvaluateType != null).Select(f => (f.MarkEvaluateType.Value, f.IsMissPress)).ToArray();
            Assert.Equal([(MarkEvaluateType.Cool, false), (MarkEvaluateType.Worst, false), (MarkEvaluateType.Safe, false)], evaluations);
            var scores = frames.Where(f => f.Score != null).ToArray();
            Assert.NotEmpty(scores);
            Assert.True(scores[^1].Score > 0);
            // 時間の順に並ぶ
            Assert.Equal(frames.Select(f => f.Time).Order(), frames.Select(f => f.Time));
        }

        [Fact]
        public void Ghost_HoldIsJudgedAtPressAndRelease()
        {
            var hold = new ExMarkData(400, 225, 0, 1, 2, ButtonType.Circle, 0);
            var frames = new List<(float, ButtonType[], ButtonType[])>();
            for (var frame = 0; frame <= 180; frame++)
            {
                var time = frame / 60f;
                frames.Add((time, frame == 60 ? [ButtonType.Circle] : [], frame == 120 ? [ButtonType.Circle] : []));
            }

            var ghost = Ghost([hold], [], Record([.. frames]));

            Assert.Equal([MarkEvaluateType.Cool, MarkEvaluateType.Cool], ghost.Where(f => f.MarkEvaluateType != null).Select(f => f.MarkEvaluateType.Value));
        }

        [Fact]
        public void Ghost_MissesLowerLife()
        {
            MarkData[] marks = [.. Enumerable.Range(1, 4).Select(i => ChartFiles.Mark(i, ButtonType.Circle))];

            var frames = Ghost(marks, [new ChangeVolumeEvent(0, 100, 0)], Play(5));

            Assert.Equal(4, frames.Count(f => f.MarkEvaluateType == MarkEvaluateType.Worst));
            var lives = frames.Where(f => f.Life != null).Select(f => f.Life.Value).ToArray();
            Assert.Equal(lives.OrderDescending(), lives);
            Assert.True(lives[^1] < lives[0]);
        }
    }
}
