using PPDCore;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ゲーム本体（PPDCore）の、描画を使わない部分（リプレイの記録と再生、ボタンの入れ替え、同時押し、凸包）のテストです。
    /// </summary>
    public class PPDCoreTests
    {
        static bool[] Bits(params int[] on) => [.. Enumerable.Range(0, 10).Select(on.Contains)];

        static PPDPlayRecorder Record()
        {
            var recorder = new PPDPlayRecorder();
            recorder.Update(0.5f, 500, new int[10], Bits(), Bits());
            var counts = new int[10];
            counts[2] = 3;
            counts[9] = 65535;
            recorder.Update(1.25f, 1250, counts, Bits(2), Bits(9));
            recorder.Update(2f, 2000, new int[10], Bits(), Bits(0, 1, 8));
            return recorder;
        }

        static void AssertSame(PPDPlayInput expected, PPDPlayInput actual)
        {
            Assert.Equal(expected.ToString(), actual.ToString());
            Assert.Equal(expected.CurrentTime, actual.CurrentTime);
        }

        [Fact]
        public void Replay_BytesRoundTrip()
        {
            var recorder = Record();

            var decoder = PPDPlayDecorder.FromBytes(recorder.GetBytes());

            Assert.Equal(3, decoder.Length);
            var inputs = recorder.Inputs;
            var pressCount = new int[10];
            var pressed = new bool[10];
            var released = new bool[10];
            foreach (var expected in inputs)
            {
                Assert.True(decoder.Update(expected.Time, pressCount, pressed, released, out var time, out var currentTime));
                Assert.Equal(expected.Time, time);
                Assert.Equal(expected.CurrentTime, currentTime);
                Assert.Equal(expected.PressCount.Select(c => (int)c), pressCount);
                Assert.Equal(expected.Pressed, pressed);
                Assert.Equal(expected.Released, released);
            }
            Assert.True(decoder.IsEnd);
            Assert.False(decoder.Update(10, pressCount, pressed, released, out _, out _));
        }

        [Fact]
        public void Replay_FileRoundTrip()
        {
            var path = Path.GetTempFileName();
            try
            {
                var recorder = Record();
                recorder.ToFile(path);

                var decoder = PPDPlayDecorder.FromFile(path);

                Assert.Equal(3, decoder.Length);
                var fromRecorder = PPDPlayDecorder.FromRecorder(recorder);
                Assert.Equal(decoder.Length, fromRecorder.Length);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Replay_WaitsUntilInputTime()
        {
            var decoder = PPDPlayDecorder.FromRecorder(Record());
            var pressCount = new int[10];
            var pressed = new bool[10];
            var released = new bool[10];

            // 記録した時刻（から 1/120 秒前まで）になるまでは進まない
            Assert.False(decoder.Update(0.4f, pressCount, pressed, released, out var time, out _));
            Assert.Equal(0.4f, time);
            Assert.True(decoder.Update(0.5f - 1 / 240f, pressCount, pressed, released, out time, out _));
            Assert.Equal(0.5f, time);
            Assert.Equal(1, decoder.Position);

            decoder.Reset();
            Assert.Equal(0, decoder.Position);
            Assert.True(PPDPlayDecorder.FromInputs([]).IsEnd);
        }

        [Fact]
        public void Replay_UnsupportedVersion_Throws()
        {
            var stream = new MemoryStream();
            using (var writer = new PPDPack.PackWriter(stream))
            {
                foreach (var file in writer.Write(["Version"]))
                {
                    file.WriteByte(1);
                }
            }

            Assert.Throws<Exception>(() => PPDPlayDecorder.FromBytes(stream.ToArray()));
        }

        [Fact]
        public void PlayInput_CompressesOnlyPressedCounts()
        {
            var counts = new ushort[10];
            counts[1] = 2;
            var input = new PPDPlayInput(1, 2, counts, Bits(1), Bits());

            // 押しているかの 2 バイト + 押しているボタンの数 2 バイト
            Assert.Equal(4, input.PressCountCompressed.Length);
            Assert.Equal(2, input.PressedCompressed.Length);
            Assert.False(input.IsEmpty);
            Assert.True(new PPDPlayInput(0, 0, new ushort[10], Bits(), Bits()).IsEmpty);
            Assert.Equal("T:1, PC:0,2,0,0,0,0,0,0,0,0, P:0,1,0,0,0,0,0,0,0,0, R:0,0,0,0,0,0,0,0,0,0", input.ToString());

            var fromBytes = new PPDPlayInput(1, 2, counts, input.PressedCompressed, input.ReleasedCompressed);
            Assert.Equal(input.ToString(), fromBytes.ToString());
        }

        [Theory]
        [InlineData(RandomChangeManager.RandomType.None)]
        [InlineData(RandomChangeManager.RandomType.Symbol)]
        [InlineData(RandomChangeManager.RandomType.Direction)]
        [InlineData(RandomChangeManager.RandomType.LR)]
        [InlineData(RandomChangeManager.RandomType.All)]
        public void RandomChange_IsPermutationInsideGroups(RandomChangeManager.RandomType type)
        {
            for (var n = 0; n < 20; n++)
            {
                var manager = new RandomChangeManager();
                manager.Initialize(type);
                var mapped = Enumerable.Range(0, 10).Select(i => (int)manager[(ButtonType)i]).ToArray();

                // どれも 1 回ずつ使い、元に戻せる
                Assert.Equal(Enumerable.Range(0, 10), mapped.Order());
                Assert.All(Enumerable.Range(0, 10), i => Assert.Equal(i, manager.Invert(mapped[i])));
                // R と L は R と L の中でだけ入れ替わる
                Assert.Equal([8, 9], mapped[8..].Order());
                if (type == RandomChangeManager.RandomType.None)
                {
                    Assert.Equal(Enumerable.Range(0, 10), mapped);
                }
                if (!type.HasFlag(RandomChangeManager.RandomType.Direction))
                {
                    Assert.Equal(type.HasFlag(RandomChangeManager.RandomType.Symbol) ? [0, 1, 2, 3] : Enumerable.Range(0, 4), mapped[..4].Order());
                    Assert.Equal([4, 5, 6, 7], mapped[4..8]);
                }
            }
        }

        [Fact]
        public void SameTimingMarks_TracksButtons()
        {
            var marks = new SameTimingMarks();
            var circle = new MarkData(0, 0, 0, 1, ButtonType.Circle, 0);
            var square = new MarkData(0, 0, 0, 1, ButtonType.Square, 0);
            var r = new MarkData(0, 0, 0, 1, ButtonType.R, 0);

            marks.Add(circle);
            marks.Add(square);
            Assert.Equal(2, marks.Count);
            Assert.True(marks.Contains(ButtonType.Circle));
            Assert.False(marks.Contains(ButtonType.Cross));
            Assert.False(marks.IsSlideButton(ButtonType.Circle));

            marks.Add(r);
            // R か L があるときは、○と△もスライドの一部
            Assert.True(marks.IsSlideButton(ButtonType.Circle));
            Assert.True(marks.IsSlideButton(ButtonType.R));
            Assert.True(marks.IsSlideButton(ButtonType.L));
            Assert.False(marks.IsSlideButton(ButtonType.Square));

            // 初期化の順番の後ろのものが先
            marks.Sort([ButtonType.Square, ButtonType.Cross, ButtonType.Circle, ButtonType.Triangle, ButtonType.Left, ButtonType.Down, ButtonType.Right, ButtonType.Up, ButtonType.R, ButtonType.L]);
            Assert.Equal([r, circle, square], marks);
            Assert.Same(r, marks[0]);

            marks.Remove(r);
            Assert.False(marks.Contains(ButtonType.R));
            marks.Clear();
            Assert.Equal(0, marks.Count);
            Assert.Equal(0, marks.SameTimings);
        }

        [Fact]
        public void ConvexHull_KeepsOuterPoints()
        {
            Vector2[] points = [new(0, 0), new(2, 0), new(1, 1), new(2, 2), new(0, 2), new(1, 0.5f)];

            var hull = ConvexHull.Convex_Hull(points);

            Assert.Equal(4, hull.Length);
            Assert.Equal([new Vector2(0, 0), new Vector2(0, 2), new Vector2(2, 0), new Vector2(2, 2)], hull.OrderBy(p => p.X).ThenBy(p => p.Y));
        }
    }
}
