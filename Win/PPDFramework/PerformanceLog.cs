using System;
using System.IO;

namespace PPDFramework
{
    /// <summary>
    /// 性能の計測結果（GC、1 フレームの処理時間）を performance_log.txt に出力します。
    /// </summary>
    /// <remarks>
    /// 設定の collectlog が 1 のとき、または環境変数 PPD_PERF_LOG（以前の名前の PPD_GC_LOG も可）が 1 のときに出力する。
    /// </remarks>
    static class PerformanceLog
    {
        const string logFileName = "performance_log.txt";

        /// <summary>
        /// ログを出力するかどうかを取得します。
        /// </summary>
        public static bool IsEnabled()
        {
            return PPDSetting.Setting?.CollectLog == true
                || Environment.GetEnvironmentVariable("PPD_PERF_LOG") == "1"
                || Environment.GetEnvironmentVariable("PPD_GC_LOG") == "1";
        }

        /// <summary>
        /// 1 行を出力します。失敗しても例外は投げません（ゲームは続ける）。
        /// </summary>
        public static void Write(string log)
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
    }
}
