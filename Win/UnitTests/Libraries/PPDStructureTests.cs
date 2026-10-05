using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 譜面のファイル（.ppd のマークとパラメーター、.evd、サウンドセット）の読み書きのテストです。
    /// </summary>
    public sealed class PPDStructureTests : IDisposable
    {
        readonly string dir = Directory.CreateTempSubdirectory("PPDStructureTest").FullName;

        public void Dispose()
        {
            Directory.Delete(dir, true);
        }

        static MarkData Mark(float time, ButtonType type, uint id, params (string Key, string Value)[] parameters)
        {
            var mark = new MarkData(10, 20, 0.5f, time, type, id);
            foreach (var (key, value) in parameters)
            {
                mark.AddParameter(key, value);
            }
            return mark;
        }

        static MarkDataBase[] RoundTrip(MarkData[] marks)
        {
            var stream = new MemoryStream();
            PPDWriter.Write(stream, marks);
            stream.Position = 0;
            return PPDReader.Read(stream);
        }

        [Fact]
        public void Marks_WithParameters_RoundTrip()
        {
            var read = RoundTrip([
                Mark(1, ButtonType.Circle, 1, ("a", "1"), ("b", "x y")),
                new ExMarkData(30, 40, 1, 2, 3, ButtonType.L, 2),
                // ID が 0 のマークのパラメーターは書かない
                Mark(4, ButtonType.Up, 0, ("lost", "1")),
            ]);

            Assert.Equal(3, read.Length);
            Assert.Equal([("a", "1"), ("b", "x y")], read[0].Parameters.Select(p => (p.Key, p.Value)));
            Assert.Equal(2.0f, read[1].Time);
            Assert.Equal(3.0f, ((ExMarkData)read[1]).EndTime);
            Assert.Equal(2u, read[1].ID);
            Assert.Empty(read[2].Parameters);
        }

        [Fact]
        public void Parameters_WithJapaneseText_RoundTrip()
        {
            // UTF-8 で 1 文字 3 バイトになる文字（以前は文字数を長さとして書き、読めなくなっていた）
            var read = RoundTrip([Mark(1, ButtonType.Circle, 1, ("歌詞", "こんにちは")), Mark(2, ButtonType.Cross, 2, ("k", "v"))]);

            Assert.Equal(("歌詞", "こんにちは"), (read[0].Parameters[0].Key, read[0].Parameters[0].Value));
            Assert.Equal(("k", "v"), (read[1].Parameters[0].Key, read[1].Parameters[0].Value));
        }

        [Fact]
        public void OldFiles_WithCharacterCountLength_AreRead()
        {
            // 以前の書き方（長さが文字数）のファイルも読める
            var stream = new MemoryStream();
            PPDWriter.Write(stream, [Mark(1, ButtonType.Circle, 5), Mark(2, ButtonType.Cross, 6)]);
            void Block(uint id, string xml)
            {
                stream.Write(BitConverter.GetBytes(id));
                stream.Write(BitConverter.GetBytes(xml.Length));
                stream.Write(System.Text.Encoding.UTF8.GetBytes(xml));
            }
            stream.Write(BitConverter.GetBytes(float.NaN));
            stream.Write(BitConverter.GetBytes(2));
            Block(5, "<Root>\n  <Parameter Key=\"歌詞\" Value=\"あいう\" />\n</Root>");
            Block(6, "<Root>\n  <Parameter Key=\"k\" Value=\"v\" />\n</Root>");
            stream.Position = 0;

            var read = PPDReader.Read(stream);

            Assert.Equal(("歌詞", "あいう"), (read[0].Parameters[0].Key, read[0].Parameters[0].Value));
            Assert.Equal(("k", "v"), (read[1].Parameters[0].Key, read[1].Parameters[0].Value));
        }

        [Fact]
        public void PpdFile_PathOverloads()
        {
            var path = Path.Combine(dir, "a.ppd");
            PPDWriter.Write(path, [Mark(1, ButtonType.Square, 0)]);

            Assert.Single(PPDReader.Read(path));
            Assert.Empty(PPDReader.Read(Path.Combine(dir, "missing.ppd")));

            File.WriteAllText(path, "not ppd");
            var signError = false;
            Assert.Empty(PPDReader.Read(new MemoryStream(File.ReadAllBytes(path)), ref signError));
            Assert.True(signError);
        }

        [Fact]
        public void Evd_PathAndInvalidValues()
        {
            var path = Path.Combine(dir, "a.evd");
            EVDWriter.Write(path, [new ChangeBPMEvent(0, 150), new ChangeBPMEvent(1, -1), new RapidChangeBPMEvent(2, -5, true), new ChangeVolumeEvent(3, 80, 1)]);

            var events = EVDReader.Read(path);

            // 負の BPM は読まない
            Assert.Equal([0f, 3f], events.Select(e => e.Time));
            Assert.Empty(EVDReader.Read(Path.Combine(dir, "missing.evd")));
        }

        [Fact]
        public void SoundSet_ReadsLines()
        {
            var path = Path.Combine(dir, "soundset.txt");
            File.WriteAllText(path, "a.wav\r\nb.wav\rc.wav\n");
            Assert.Equal(["a.wav", "b.wav", "c.wav"], SoundSetReader.Read(path));

            File.WriteAllText(path, "  \r\n");
            Assert.Empty(SoundSetReader.Read(path));
            Assert.Empty(SoundSetReader.Read(Path.Combine(dir, "missing.txt")));
        }

        [Fact]
        public void Scd_PathOverloads()
        {
            var path = Path.Combine(dir, "a.scd");
            SCDWriter.Write(path, [new SCDData(1.5f, ButtonType.Circle, 3)]);

            var read = Assert.Single(SCDReader.Read(path));

            Assert.Equal((1.5f, ButtonType.Circle, 3), (read.Time, read.ButtonType, (int)read.SoundIndex));
            Assert.Empty(SCDReader.Read(Path.Combine(dir, "missing.scd")));
        }
    }
}
