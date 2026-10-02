using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

namespace PPDFramework
{
    /// <summary>
    /// プレイ中の GC による引っかかりを減らすため、GC を行う時期を制御するクラスです。
    /// </summary>
    /// <remarks>
    /// シーンの切り替えやリトライの前にまとめて GC を行い、プレイ中は止まる形のフル GC（第 2 世代のブロッキング GC）を避ける。
    /// 第 0・第 1 世代の GC はプレイ中も起きる（ヒープが小さければ短い）。
    /// ログは PerformanceLog（performance_log.txt）に出力する。プレイの区間は、1 フレームの処理時間の記録（FrameTimeStatistics）にも使う。
    /// </remarks>
    public static class GCPolicy
    {
        static readonly Lock lockObject = new();
        static readonly Stopwatch playStopwatch = new();
        static bool playing;
        static bool logEnabled;
        static GCLatencyMode previousLatencyMode;
        static PlayStatistics playStart;
        static TimeSpan maxPause;

        // この時間以上止まった GC は、詳細をログに出す
        static readonly TimeSpan longPauseThreshold = TimeSpan.FromMilliseconds(8);
        // GC の種類ごとに直近の GC を調べる（フレームの間に同じ種類の GC が 2 回以上起きた場合は、最後の 1 回だけが見える）
        static readonly GCKind[] gcKinds = [GCKind.Ephemeral, GCKind.FullBlocking, GCKind.Background];
        static readonly long[] lastGCIndices = new long[gcKinds.Length];
        static readonly List<string> longPauses = [];

        /// <summary>
        /// プレイ中かどうかを取得します。
        /// </summary>
        public static bool Playing
        {
            get
            {
                return playing;
            }
        }

        /// <summary>
        /// フル GC を行い、ファイナライザーの完了を待ちます。シーンの切り替えやリトライの前など、止まってよいときに呼びます。
        /// </summary>
        /// <param name="reason">ログに出す理由。</param>
        public static void CollectNow(string reason)
        {
            var stopwatch = Stopwatch.StartNew();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            // ファイナライザーで解放されるもの（テクスチャなど）も、ここで回収しておく
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            stopwatch.Stop();
            lock (lockObject)
            {
                if (playing)
                {
                    // プレイの計測には、この GC を含めない
                    ResetPlayStatistics();
                }
            }
            if (PerformanceLog.IsEnabled())
            {
                PerformanceLog.Write(String.Format("collect reason={0} time={1:F1}ms heap={2:F1}MB",
                    reason, stopwatch.Elapsed.TotalMilliseconds, GC.GetTotalMemory(false) / 1024.0 / 1024.0));
            }
        }

        /// <summary>
        /// プレイを開始します。プレイ中は、止まる形のフル GC を避けるモードにします。
        /// </summary>
        public static void BeginPlay()
        {
            lock (lockObject)
            {
                if (playing)
                {
                    return;
                }
                playing = true;
                logEnabled = PerformanceLog.IsEnabled();
                previousLatencyMode = GCSettings.LatencyMode;
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                ResetPlayStatistics();
            }
        }

        /// <summary>
        /// プレイを終了し、GC のモードを元に戻します。ログが有効なら、プレイ中の GC の統計を出力します。
        /// </summary>
        /// <param name="reason">ログに出す理由。</param>
        public static void EndPlay(string reason)
        {
            string log;
            string[] pauses;
            string[] frames;
            lock (lockObject)
            {
                if (!playing)
                {
                    return;
                }
                // 最後のフレーム以降の GC も拾う（playing を下ろす前に呼ぶ）
                Sample();
                playing = false;
                FrameTimeStatistics.Stop();
                GCSettings.LatencyMode = previousLatencyMode;
                if (!logEnabled)
                {
                    return;
                }
                pauses = [.. longPauses];
                frames = FrameTimeStatistics.Summarize();
                var end = PlayStatistics.Capture();
                var seconds = playStopwatch.Elapsed.TotalSeconds;
                var allocatedMB = (end.AllocatedBytes - playStart.AllocatedBytes) / 1024.0 / 1024.0;
                log = String.Format("play reason={0} time={1:F1}s allocated={2:F1}MB ({3:F2}MB/s) gen0={4} gen1={5} gen2={6} totalPause={7:F1}ms maxPause={8:F1}ms heap={9:F1}MB",
                    reason, seconds, allocatedMB, seconds > 0 ? allocatedMB / seconds : 0,
                    end.Gen0Count - playStart.Gen0Count, end.Gen1Count - playStart.Gen1Count, end.Gen2Count - playStart.Gen2Count,
                    (end.TotalPause - playStart.TotalPause).TotalMilliseconds, maxPause.TotalMilliseconds,
                    GC.GetTotalMemory(false) / 1024.0 / 1024.0);
            }
            PerformanceLog.Write(log);
            PerformanceLog.Write(String.Format("  jit warmup: methods={0} time={1:F0}ms", JitWarmup.PreparedCount, JitWarmup.ElapsedMilliseconds));
            foreach (var line in frames)
            {
                PerformanceLog.Write(line);
            }
            foreach (var pause in pauses)
            {
                PerformanceLog.Write(pause);
            }
        }

        /// <summary>
        /// プレイ中の GC の停止時間を計測します。ログが有効なときだけ、毎フレーム呼ばれることを想定しています。
        /// </summary>
        public static void Sample()
        {
            if (!playing || !logEnabled)
            {
                return;
            }
            for (var i = 0; i < gcKinds.Length; i++)
            {
                var info = GC.GetGCMemoryInfo(gcKinds[i]);
                if (info.Index == lastGCIndices[i])
                {
                    continue;
                }
                lastGCIndices[i] = info.Index;
                var longest = TimeSpan.Zero;
                foreach (var pause in info.PauseDurations)
                {
                    if (pause > longest)
                    {
                        longest = pause;
                    }
                }
                if (longest > maxPause)
                {
                    maxPause = longest;
                }
                if (longest >= longPauseThreshold)
                {
                    // 背景 GC は止まる区間が 2 つある（開始時と終了時）ので、両方を出す
                    var durations = info.PauseDurations;
                    longPauses.Add(String.Format("  long pause at={0:F1}s pause={1:F1}ms ({2:F1}ms + {3:F1}ms) kind={4} gen={5} compacted={6} concurrent={7} promoted={8:F1}MB heap={9:F1}MB pauseTimePercentage={10:F1}%",
                        playStopwatch.Elapsed.TotalSeconds, longest.TotalMilliseconds,
                        durations.Length > 0 ? durations[0].TotalMilliseconds : 0, durations.Length > 1 ? durations[1].TotalMilliseconds : 0,
                        gcKinds[i], info.Generation, info.Compacted, info.Concurrent,
                        info.PromotedBytes / 1024.0 / 1024.0, info.HeapSizeBytes / 1024.0 / 1024.0, info.PauseTimePercentage));
                }
            }
        }

        private static void ResetPlayStatistics()
        {
            playStart = PlayStatistics.Capture();
            playStopwatch.Restart();
            for (var i = 0; i < gcKinds.Length; i++)
            {
                lastGCIndices[i] = GC.GetGCMemoryInfo(gcKinds[i]).Index;
            }
            maxPause = TimeSpan.Zero;
            longPauses.Clear();
            FrameTimeStatistics.Begin(logEnabled);
        }

        readonly struct PlayStatistics(int gen0Count, int gen1Count, int gen2Count, TimeSpan totalPause, long allocatedBytes)
        {
            public readonly int Gen0Count = gen0Count;
            public readonly int Gen1Count = gen1Count;
            public readonly int Gen2Count = gen2Count;
            public readonly TimeSpan TotalPause = totalPause;
            public readonly long AllocatedBytes = allocatedBytes;

            public static PlayStatistics Capture()
            {
                return new PlayStatistics(GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2),
                    GC.GetTotalPauseDuration(), GC.GetTotalAllocatedBytes(false));
            }
        }
    }
}
