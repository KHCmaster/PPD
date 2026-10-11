using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PPDFramework
{
    /// <summary>
    /// プレイ中の 1 フレームの処理時間（更新、描画、待ち、Present）を記録し、プレイの終わりに分布をログに出します。
    /// </summary>
    /// <remarks>
    /// 記録する区間は GCPolicy のプレイの区間と同じ。記録中は確保をしない（配列は使い回す）。
    /// </remarks>
    static class FrameTimeStatistics
    {
        // 144fps で約 15 分ぶん。超えた分は記録しない
        const int Capacity = 1 << 17;

        static readonly float[] updateTimes = new float[Capacity];
        static readonly float[] drawTimes = new float[Capacity];
        static readonly float[] sleepTimes = new float[Capacity];
        static readonly float[] presentTimes = new float[Capacity];
        static readonly float[] intervals = new float[Capacity];
        static int count;
        static long lastFrameStart;
        static long sessionStart;
        static int lastGCCount;

        // 更新か描画がこの時間以上かかったフレームは、区間ごとの内訳を残す
        const float SpikeThreshold = 4;
        const int SpikeCapacity = 256;
        static readonly Spike[] spikes = new Spike[SpikeCapacity];
        static int spikeCount;
        static int spikeOverflow;
        static volatile bool recording;
        // 記録の区間の番号。フレームの途中で区間が変わった（リトライなど）フレームは記録しない
        static volatile int session;

        /// <summary>
        /// フレームの開始時に、記録の区間の番号を取得します（記録していないときは 0）。
        /// </summary>
        public static int CurrentSession
        {
            get
            {
                return recording ? session : 0;
            }
        }

        /// <summary>
        /// 記録を始めます（すでに記録中なら、最初からやり直します）。
        /// </summary>
        public static void Begin(bool enabled)
        {
            recording = false;
            count = 0;
            lastFrameStart = 0;
            sessionStart = Stopwatch.GetTimestamp();
            lastGCCount = GC.CollectionCount(0);
            spikeCount = 0;
            spikeOverflow = 0;
            session++;
            recording = enabled;
        }

        /// <summary>
        /// 記録を止めます。
        /// </summary>
        public static void Stop()
        {
            recording = false;
        }

        /// <summary>
        /// 1 フレームの時刻（Stopwatch.GetTimestamp の値）を記録します。描画しなかったフレームは、描画以降の時刻を更新の終わりと同じにします。
        /// </summary>
        public static void Record(int frameSession, long start, long afterUpdate, long afterDraw, long afterSleep, long afterPresent)
        {
            if (!recording || frameSession == 0 || frameSession != session || count >= Capacity)
            {
                return;
            }
            updateTimes[count] = ToMilliseconds(afterUpdate - start);
            drawTimes[count] = ToMilliseconds(afterDraw - afterUpdate);
            sleepTimes[count] = ToMilliseconds(afterSleep - afterDraw);
            presentTimes[count] = ToMilliseconds(afterPresent - afterSleep);
            intervals[count] = lastFrameStart == 0 ? float.NaN : ToMilliseconds(start - lastFrameStart);
            lastFrameStart = start;
            // 前のフレームの記録からこのフレームまでに起きた GC の回数（第 0 世代の数には、上の世代の GC も含まれる）
            var gcCount = GC.CollectionCount(0);
            var gcs = gcCount - lastGCCount;
            lastGCCount = gcCount;
            if (updateTimes[count] >= SpikeThreshold || drawTimes[count] >= SpikeThreshold)
            {
                if (spikeCount < SpikeCapacity)
                {
                    ref var spike = ref spikes[spikeCount++];
                    spike.Time = ToMilliseconds(start - sessionStart) / 1000;
                    spike.Update = updateTimes[count];
                    spike.Draw = drawTimes[count];
                    spike.GCs = gcs;
                    spike.Sections ??= new float[FrameProfiler.SectionCount];
                    FrameProfiler.CopyMilliseconds(spike.Sections);
                    spike.Notes = FrameProfiler.GetNotes();
                }
                else
                {
                    spikeOverflow++;
                }
            }
            count++;
        }

        /// <summary>
        /// 記録した分布をログの行にします。
        /// </summary>
        public static string[] Summarize()
        {
            var n = count;
            if (n == 0)
            {
                return [];
            }
            var cpu = new float[n];
            for (var i = 0; i < n; i++)
            {
                cpu[i] = updateTimes[i] + drawTimes[i];
            }
            var interval = intervals.Take(n).Where(x => !float.IsNaN(x)).ToArray();
            var lines = new List<string>
            {
                String.Format("  frame frames={0} (ms: p50 / p95 / p99 / max / mean)", n),
                Format("cpu(update+draw)", cpu),
                Format("update", updateTimes.AsSpan(0, n).ToArray()),
                Format("draw", drawTimes.AsSpan(0, n).ToArray()),
                Format("sleep", sleepTimes.AsSpan(0, n).ToArray()),
                Format("present", presentTimes.AsSpan(0, n).ToArray()),
                Format("interval", interval),
            };
            if (interval.Length > 0)
            {
                // 間隔が中央値の 1.5 倍を超えたフレーム（表示が 1 回以上抜けた可能性がある）
                var median = Percentile([.. interval.OrderBy(x => x)], 0.5);
                lines.Add(String.Format("  frame slow frames (interval > {0:F2}ms): {1}", median * 1.5, interval.Count(x => x > median * 1.5)));
            }
            if (spikeCount > 0)
            {
                // 区間は入れ子になりうる（包含時間）。other は、更新と描画のうち、どの区間にも入らなかった時間の目安
                lines.Add(String.Format("  spike frames (update or draw >= {0}ms): {1}{2}", SpikeThreshold, spikeCount + spikeOverflow,
                    spikeOverflow > 0 ? String.Format(" (first {0} shown)", spikeCount) : ""));
                var names = Enum.GetNames<FrameSection>();
                for (var i = 0; i < spikeCount; i++)
                {
                    var spike = spikes[i];
                    var sections = String.Join(" ", names.Select((name, j) => spike.Sections[j] >= 0.05f ? String.Format("{0}={1:F2}", name, spike.Sections[j]) : null).Where(s => s != null));
                    lines.Add(String.Format("    spike at={0:F2}s update={1:F2} draw={2:F2} gc={3} {4}", spike.Time, spike.Update, spike.Draw, spike.GCs, sections));
                    if (!String.IsNullOrEmpty(spike.Notes))
                    {
                        lines.Add("      " + spike.Notes);
                    }
                }
            }
            return [.. lines];
        }

        struct Spike
        {
            public float Time;
            public float Update;
            public float Draw;
            public int GCs;
            public float[] Sections;
            public string Notes;
        }

        private static string Format(string name, float[] values)
        {
            if (values.Length == 0)
            {
                return String.Format("  frame {0,-17} -", name);
            }
            var sorted = values.OrderBy(x => x).ToArray();
            return String.Format("  frame {0,-17} {1,6:F3} / {2,6:F3} / {3,6:F3} / {4,7:F3} / {5,6:F3}",
                name, Percentile(sorted, 0.5), Percentile(sorted, 0.95), Percentile(sorted, 0.99), sorted[^1], values.Average());
        }

        private static float Percentile(float[] sorted, double p)
        {
            return sorted[Math.Min(sorted.Length - 1, (int)(sorted.Length * p))];
        }

        private static float ToMilliseconds(long ticks)
        {
            return (float)(ticks * 1000.0 / Stopwatch.Frequency);
        }
    }
}
