using PPDFramework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// フレームの時間の計測（FrameProfiler、FrameTimeStatistics）と、プレイ中の GC の設定（GCPolicy）のテストです。
    /// </summary>
    public class PerformanceStatisticsTests
    {
        static readonly long ticksPerMillisecond = Stopwatch.Frequency / 1000;

        static void Spin(double milliseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalMilliseconds < milliseconds)
            {
            }
        }

        static float[] Sections()
        {
            var sections = new float[FrameProfiler.SectionCount];
            FrameProfiler.CopyMilliseconds(sections);
            return sections;
        }

        [Fact]
        public void FrameProfiler_MeasuresSectionsOnTheMainThread()
        {
            try
            {
                FrameProfiler.BeginFrame(true);
                Assert.True(FrameProfiler.IsRecording);
                using (FrameProfiler.Measure(FrameSection.Text))
                {
                    // 入れ子は外側だけを数える
                    using (FrameProfiler.Measure(FrameSection.Text))
                    {
                        Spin(2);
                    }
                }
                FrameProfiler.Note("文字");
                FrameProfiler.Note("画像");

                var sections = Sections();
                Assert.InRange(sections[(int)FrameSection.Text], 2, 1000);
                Assert.Equal(0, sections[(int)FrameSection.Marks]);
                Assert.Equal("文字 画像", FrameProfiler.GetNotes());

                // ほかのスレッドからは数えない
                var recordingOnOtherThread = true;
                var thread = new Thread(() =>
                {
                    recordingOnOtherThread = FrameProfiler.IsRecording;
                    using (FrameProfiler.Measure(FrameSection.Marks))
                    {
                        Spin(1);
                    }
                    FrameProfiler.Note("別のスレッド");
                });
                thread.Start();
                thread.Join();
                Assert.False(recordingOnOtherThread);
                Assert.Equal(0, Sections()[(int)FrameSection.Marks]);
                Assert.Equal("文字 画像", FrameProfiler.GetNotes());

                // メモは長くなりすぎたら足さない
                for (var i = 0; i < 100; i++)
                {
                    FrameProfiler.Note("0123456789");
                }
                Assert.InRange(FrameProfiler.GetNotes().Length, 200, 220);

                // フレームの始めに消える
                FrameProfiler.BeginFrame(false);
                Assert.False(FrameProfiler.IsRecording);
                using (FrameProfiler.Measure(FrameSection.Text))
                {
                    Spin(1);
                }
                FrameProfiler.Note("x");
                Assert.All(Sections(), s => Assert.Equal(0, s));
                Assert.Equal("", FrameProfiler.GetNotes());
            }
            finally
            {
                FrameProfiler.BeginFrame(false);
            }
        }

        static long Ms(double milliseconds) => (long)(milliseconds * ticksPerMillisecond);

        // 更新、描画、待ち、表示にかかった時間（ミリ秒）で 1 フレームを記録する
        static long RecordFrame(int session, long start, double update, double draw, double sleep = 10, double present = 1)
        {
            var afterUpdate = start + Ms(update);
            var afterDraw = afterUpdate + Ms(draw);
            var afterSleep = afterDraw + Ms(sleep);
            FrameTimeStatistics.Record(session, start, afterUpdate, afterDraw, afterSleep, afterSleep + Ms(present));
            return start + Ms(16);
        }

        [Fact]
        public void FrameTimeStatistics_SummarizesFramesAndSpikes()
        {
            try
            {
                FrameTimeStatistics.Begin(true);
                var session = FrameTimeStatistics.CurrentSession;
                Assert.NotEqual(0, session);
                var start = Stopwatch.GetTimestamp();
                for (var i = 0; i < 9; i++)
                {
                    start = RecordFrame(session, start, 1, 2);
                }
                // 重いフレームは、区間ごとの内訳とメモを残す
                FrameProfiler.BeginFrame(true);
                FrameProfiler.Note("重い処理");
                start = RecordFrame(session, start + Ms(20), 6, 2);
                FrameProfiler.BeginFrame(false);
                // 前の区間のフレームは記録しない
                RecordFrame(session - 1, start, 100, 100);
                RecordFrame(0, start, 100, 100);

                var lines = FrameTimeStatistics.Summarize();

                Assert.Contains("frames=10", lines[0]);
                Assert.StartsWith("  frame cpu(update+draw)", lines[1]);
                Assert.Contains(lines, l => l.StartsWith("  frame update ") && l.Contains("6.000"));
                // 間隔が中央値の 1.5 倍を超えたフレームは 1 つ
                Assert.Contains(lines, l => l.StartsWith("  frame slow frames (interval > 24.00ms): 1"));
                Assert.Contains("  spike frames (update or draw >= 4ms): 1", lines);
                Assert.Contains(lines, l => l.StartsWith("    spike at=") && l.Contains("update=6.00 draw=2.00"));
                Assert.Contains("      重い処理", lines);

                // 止めたら記録しない
                FrameTimeStatistics.Stop();
                Assert.Equal(0, FrameTimeStatistics.CurrentSession);
                RecordFrame(session, start, 1, 1);
                Assert.Contains("frames=10", FrameTimeStatistics.Summarize()[0]);

                // 記録しないで始めると何もない
                FrameTimeStatistics.Begin(false);
                Assert.Empty(FrameTimeStatistics.Summarize());

                // 1 フレームだけなら間隔はない
                FrameTimeStatistics.Begin(true);
                RecordFrame(FrameTimeStatistics.CurrentSession, Stopwatch.GetTimestamp(), 1, 1);
                Assert.Contains("  frame interval          -", FrameTimeStatistics.Summarize());
            }
            finally
            {
                FrameTimeStatistics.Begin(false);
            }
        }

        [Fact]
        public void GCPolicy_LogsPlayToPerformanceLog()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var latencyMode = GCSettings.LatencyMode;
            Environment.SetEnvironmentVariable("PPD_PERF_LOG", "1");
            try
            {
                GCPolicy.EndPlay("not playing");
                GCPolicy.BeginPlay();
                Assert.True(GCPolicy.Playing);
                Assert.Equal(GCLatencyMode.SustainedLowLatency, GCSettings.LatencyMode);
                GCPolicy.BeginPlay();
                var session = FrameTimeStatistics.CurrentSession;
                var start = RecordFrame(session, Stopwatch.GetTimestamp(), 1, 1);
                RecordFrame(session, start, 1, 1);
                GC.Collect();
                GCPolicy.Sample();
                // プレイ中に集めると、計測をやり直す
                GCPolicy.CollectNow("テスト");
                GCPolicy.EndPlay("終わり");

                Assert.False(GCPolicy.Playing);
                Assert.Equal(latencyMode, GCSettings.LatencyMode);
                var log = File.ReadAllLines(folder.Combine("performance_log.txt"));
                Assert.Contains(log, l => l.Contains("] collect reason=テスト time="));
                Assert.Contains(log, l => l.Contains("] play reason=終わり time="));
                Assert.Contains(log, l => l.Contains("]   jit warmup: methods="));
                // 集めたときに、それまでのフレームは消える
                Assert.DoesNotContain(log, l => l.Contains("frame frames="));
                Assert.DoesNotContain(log, l => l.Contains("not playing"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("PPD_PERF_LOG", null);
                GCPolicy.EndPlay("cleanup");
                GCSettings.LatencyMode = latencyMode;
            }
        }

        [Fact]
        public void GCPolicy_WithoutLog_OnlyChangesLatencyMode()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var latencyMode = GCSettings.LatencyMode;

            GCPolicy.BeginPlay();
            GCPolicy.Sample();
            GCPolicy.EndPlay("終わり");

            Assert.Equal(latencyMode, GCSettings.LatencyMode);
            Assert.False(File.Exists(folder.Combine("performance_log.txt")));
            Assert.Equal(0, FrameTimeStatistics.CurrentSession);
        }
    }
}
