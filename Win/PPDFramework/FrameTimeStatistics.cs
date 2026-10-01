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
                var median = Percentile(interval.OrderBy(x => x).ToArray(), 0.5);
                lines.Add(String.Format("  frame slow frames (interval > {0:F2}ms): {1}", median * 1.5, interval.Count(x => x > median * 1.5)));
            }
            return [.. lines];
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
