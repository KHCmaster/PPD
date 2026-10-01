using System;
using System.Diagnostics;
using System.IO;
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
    /// ログ（gc_log.txt）は、設定の collectlog が 1 のとき、または環境変数 PPD_GC_LOG が 1 のときに出力する。
    /// </remarks>
    public static class GCPolicy
    {
        const string logFileName = "gc_log.txt";

        static readonly Lock lockObject = new();
        static readonly Stopwatch playStopwatch = new();
        static bool playing;
        static bool logEnabled;
        static GCLatencyMode previousLatencyMode;
        static PlayStatistics playStart;
        static long lastGCIndex;
        static TimeSpan maxPause;

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

        private static bool IsLogEnabled()
        {
            return PPDSetting.Setting?.CollectLog == true || Environment.GetEnvironmentVariable("PPD_GC_LOG") == "1";
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
            if (IsLogEnabled())
            {
                WriteLog(String.Format("collect reason={0} time={1:F1}ms heap={2:F1}MB",
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
                logEnabled = IsLogEnabled();
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
            lock (lockObject)
            {
                if (!playing)
                {
                    return;
                }
                playing = false;
                GCSettings.LatencyMode = previousLatencyMode;
                if (!logEnabled)
                {
                    return;
                }
                Sample();
                var end = PlayStatistics.Capture();
                var seconds = playStopwatch.Elapsed.TotalSeconds;
                var allocatedMB = (end.AllocatedBytes - playStart.AllocatedBytes) / 1024.0 / 1024.0;
                log = String.Format("play reason={0} time={1:F1}s allocated={2:F1}MB ({3:F2}MB/s) gen0={4} gen1={5} gen2={6} totalPause={7:F1}ms maxPause={8:F1}ms heap={9:F1}MB",
                    reason, seconds, allocatedMB, seconds > 0 ? allocatedMB / seconds : 0,
                    end.Gen0Count - playStart.Gen0Count, end.Gen1Count - playStart.Gen1Count, end.Gen2Count - playStart.Gen2Count,
                    (end.TotalPause - playStart.TotalPause).TotalMilliseconds, maxPause.TotalMilliseconds,
                    GC.GetTotalMemory(false) / 1024.0 / 1024.0);
            }
            WriteLog(log);
        }

        /// <summary>
        /// プレイ中の最大の停止時間を計測します。ログが有効なときだけ、毎フレーム呼ばれることを想定しています。
        /// </summary>
        public static void Sample()
        {
            if (!playing || !logEnabled)
            {
                return;
            }
            var info = GC.GetGCMemoryInfo(GCKind.Any);
            if (info.Index == lastGCIndex)
            {
                return;
            }
            lastGCIndex = info.Index;
            foreach (var pause in info.PauseDurations)
            {
                if (pause > maxPause)
                {
                    maxPause = pause;
                }
            }
        }

        private static void ResetPlayStatistics()
        {
            playStart = PlayStatistics.Capture();
            playStopwatch.Restart();
            lastGCIndex = GC.GetGCMemoryInfo(GCKind.Any).Index;
            maxPause = TimeSpan.Zero;
        }

        private static void WriteLog(string log)
        {
            try
            {
                File.AppendAllText(logFileName, String.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}", DateTime.Now, log, Environment.NewLine));
            }
            catch
            {
                // ログの出力に失敗しても、ゲームは続ける
            }
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
