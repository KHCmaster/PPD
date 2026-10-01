using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDFramework
{
    /// <summary>
    /// COM オブジェクトの解放漏れを調べるための診断機能です。
    /// 環境変数 PPD_COM_DIAGNOSTICS=1 で起動したときだけ有効になり、次のことを行います。
    /// - ファイナライザーでの解放を無効にし、SharpGen のオブジェクト追跡を有効にする
    /// - D3D11 のデバイスをデバッグレイヤー付きで作り、デバイスの破棄の直前に残っているオブジェクトを書き出す
    /// - ゲームの破棄の後に、Dispose されていない COM オブジェクトの型を書き出す
    /// 結果は実行ファイルのフォルダーの com_diagnostics.log に書き出します。
    /// </summary>
    static class ComDiagnostics
    {
        const string LogFileName = "com_diagnostics.log";

        /// <summary>
        /// 有効かどうかを取得します。
        /// </summary>
        public static bool IsEnabled { get; } = Environment.GetEnvironmentVariable("PPD_COM_DIAGNOSTICS") == "1";

        /// <summary>
        /// COM オブジェクトの解放方法を設定します。最初の COM オブジェクトを作るより前に呼ぶ必要があります。
        /// </summary>
        public static void Configure()
        {
            if (IsEnabled)
            {
                SharpGen.Runtime.Configuration.EnableObjectTracking = true;
                SharpGen.Runtime.Configuration.EnableReleaseOnFinalizer = false;
                File.WriteAllText(LogPath, "", Encoding.UTF8);
            }
            else
            {
                // Dispose されなかった COM オブジェクトもファイナライザーで解放する（SharpDX のときと同じ動作）
                SharpGen.Runtime.Configuration.EnableReleaseOnFinalizer = true;
            }
        }

        /// <summary>
        /// Dispose されていない COM オブジェクトの型と数を書き出します。
        /// </summary>
        /// <param name="title"></param>
        public static void ReportActiveObjects(string title)
        {
            if (!IsEnabled)
            {
                return;
            }
            var objects = SharpGen.Runtime.Diagnostics.ObjectTracker.FindActiveObjects()
                .Select(r => r.TryGetTarget(out SharpGen.Runtime.CppObject target) ? target : null)
                .Where(o => o != null && o.NativePointer != IntPtr.Zero)
                .GroupBy(o => o.GetType().FullName)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => string.Format("  {0} x{1}", g.Key, g.Count()));
            Write(title, objects.ToArray());
        }

        static readonly System.Collections.Generic.Dictionary<object, string> trackedObjects = [];

        /// <summary>
        /// 作成した場所（スタックトレース）を記録します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="description"></param>
        public static void Track(object obj, string description)
        {
            if (!IsEnabled)
            {
                return;
            }
            lock (trackedObjects)
            {
                trackedObjects[obj] = description + Environment.NewLine + Environment.StackTrace;
            }
        }

        /// <summary>
        /// 記録を削除します。
        /// </summary>
        /// <param name="obj"></param>
        public static void UnTrack(object obj)
        {
            if (!IsEnabled)
            {
                return;
            }
            lock (trackedObjects)
            {
                trackedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// 破棄されていないオブジェクトの作成場所を書き出します。
        /// </summary>
        /// <param name="title"></param>
        public static void ReportTrackedObjects(string title)
        {
            if (!IsEnabled)
            {
                return;
            }
            lock (trackedObjects)
            {
                Write(title, trackedObjects.Values.ToArray());
            }
        }

        /// <summary>
        /// ログに書き出します。
        /// </summary>
        /// <param name="title"></param>
        /// <param name="lines"></param>
        public static void Write(string title, string[] lines)
        {
            if (!IsEnabled)
            {
                return;
            }
            var builder = new StringBuilder();
            builder.AppendLine("[" + title + "] " + lines.Length);
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }
            File.AppendAllText(LogPath, builder.ToString(), Encoding.UTF8);
        }

        private static string LogPath
        {
            get { return Path.Combine(AppContext.BaseDirectory, LogFileName); }
        }
    }
}
