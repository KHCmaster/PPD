using BMSTOPPD.Models;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.Bmstoppd
{
    public class BmsConverterTests
    {
        const int square = 0;
        const int circle = 2;

        /// <summary>
        /// 1 小節（4 拍）の秒数です。
        /// </summary>
        private static double Measure(double bpm)
        {
            return 60 / bpm * 4;
        }

        [Fact]
        public void Analyze_PlacesNotesByMeasureAndPosition()
        {
            // 1 小節目に 4 分音符を 4 つ（チャンネル 11 は □）、2 小節目の頭と真ん中に ○（チャンネル 13）
            var times = BmsConverter.Analyze("#BPM 120\n#00111:01010101\n#00213:0101\n", 0, false);

            Assert.Equal([2.0, 2.5, 3.0, 3.5], times[square]);
            Assert.Equal([4.0, 5.0], times[circle]);
            Assert.All(Enumerable.Range(0, 10).Where(i => i != square && i != circle), i => Assert.Empty(times[i]));
        }

        [Fact]
        public void Analyze_AddsTimeOffset()
        {
            var times = BmsConverter.Analyze("#BPM 120\n#00011:01\n", 1.5, false);

            Assert.Equal([1.5], times[square]);
        }

        [Fact]
        public void Analyze_WithoutBpm_Uses130()
        {
            var times = BmsConverter.Analyze("#00111:01\n", 0, false);

            Assert.Equal(Measure(130), times[square].Single(), 6);
        }

        [Fact]
        public void Analyze_SkipsZeroNotes()
        {
            var times = BmsConverter.Analyze("#BPM 120\n#00011:00000100\n", 0, false);

            Assert.Equal([1.0], times[square]);
        }

        [Fact]
        public void Analyze_MeasureScaleChangesLength()
        {
            // 0 小節目を半分の長さにする（チャンネル 02）
            var times = BmsConverter.Analyze("#BPM 120\n#00002:0.5\n#00111:01\n", 0, false);

            Assert.Equal(Measure(120) / 2, times[square].Single(), 6);
        }

        [Fact]
        public void Analyze_BpmChangeInMeasure()
        {
            // 0 小節目の真ん中で BPM を 240（16 進で F0）にする（チャンネル 03）
            var times = BmsConverter.Analyze("#BPM 120\n#00003:00F0\n#00111:01\n", 0, false);

            // 前半は 120、後半は 240 の速さ
            Assert.Equal(Measure(120) / 2 + Measure(240) / 2, times[square].Single(), 6);
        }

        [Theory]
        [InlineData(11, 0)]
        [InlineData(12, 1)]
        [InlineData(15, 4)]
        [InlineData(16, 8)]
        [InlineData(17, 9)]
        [InlineData(18, 5)]
        [InlineData(19, 6)]
        public void Analyze_MapsBmsChannelsToButtons(int channel, int button)
        {
            var times = BmsConverter.Analyze("#BPM 120\n#000" + channel + ":01\n", 0, false);

            Assert.Single(times[button]);
        }

        [Fact]
        public void Analyze_Dtx_UsesHexChannelsAndColonSpace()
        {
            // DTX は「#BPM: 120」「#00111: 01」のように、コロンの後に空白がある。チャンネル 11（16 進）が □
            var times = BmsConverter.Analyze("#BPM: 120\n#00111: 0101\n#00013: 01\n", 0, true);

            Assert.Equal([2.0, 3.0], times[square]);
            Assert.Equal([0.0], times[circle]);
        }

        [Fact]
        public void Analyze_Dtx_SkipsChannelsAfter1A()
        {
            // DTX で使うのは 11 から 1A（16 進）まで。1B 以降は無視する
            var times = BmsConverter.Analyze("#BPM: 120\n#0001B: 01\n", 0, true);

            Assert.All(times, Assert.Empty);
        }

        [Fact]
        public void Analyze_Dtx_ChannelsBelow11_Throw()
        {
            // DTX のチャンネル 0B から 10（16 進）は、範囲の確かめ（10 進の 11 から 26）を通ってしまい、ボタンの番号が負になる。
            // 変換のときは例外になり、「error」と知らせる（以前と同じ動き）
            Assert.Throws<IndexOutOfRangeException>(() => BmsConverter.Analyze("#BPM: 120\n#0000C: 01\n", 0, true));
        }

        [Fact]
        public void Convert_WritesPpdNextToSource()
        {
            using var folder = new TempFolder();
            var source = folder.Write("song.bms", "");
            var settings = Enumerable.Range(0, 10).Select(i => new MarkSetting { Name = "b" + i, X = "100", Y = (200 + i).ToString(), Angle = "90" }).ToList();

            var path = BmsConverter.Convert("#BPM 120\n#00011:0101\n#00013:01\n", source, 0, false, settings);

            Assert.Equal(folder.Combine("trans.ppd"), path);
            var bytes = File.ReadAllBytes(path);
            // 先頭の「PPD」と、ノーツごとに時間・X・Y・角度（float）とボタン（1 バイト）
            Assert.Equal("PPD"u8.ToArray(), bytes[..3]);
            Assert.Equal(3 + 3 * 17, bytes.Length);
            // 時間の順に並ぶ（同じ時間なら、ボタンの番号の大きいほうが先）
            Assert.Equal(0f, BitConverter.ToSingle(bytes, 3));
            Assert.Equal(circle, bytes[3 + 16]);
            Assert.Equal(202f, BitConverter.ToSingle(bytes, 3 + 8));
            Assert.Equal((float)Math.PI / 2, BitConverter.ToSingle(bytes, 3 + 12), 5);
            Assert.Equal(square, bytes[3 + 17 + 16]);
            Assert.Equal(1f, BitConverter.ToSingle(bytes, 3 + 34));
        }

        [Fact]
        public void Convert_OverwritesExistingOutput()
        {
            using var folder = new TempFolder();
            var source = folder.Write("song.bms", "");
            folder.Write("trans.ppd", new string('x', 1000));
            var settings = Enumerable.Range(0, 10).Select(i => new MarkSetting { Name = "b" + i, X = "0", Y = "0", Angle = "0" }).ToList();

            var path = BmsConverter.Convert("#BPM 120\n#00011:01\n", source, 0, false, settings);

            Assert.Equal(3 + 17, new FileInfo(path).Length);
        }
    }
}
