using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UITests
{
    /// <summary>
    /// テストするアプリを 1 つ起動したものです。終わったら <see cref="Dispose"/> で終了させ、作業のフォルダを消します。
    /// </summary>
    /// <remarks>
    /// アプリは、ビルドしたフォルダを一時フォルダにコピーしてから起動する。
    /// アプリが書く設定のファイル（終了時に保存するものなど）で、開発用のフォルダを変えないため。
    /// アプリは普通には閉じず、プロセスを終了させる（閉じるときに設定を保存するアプリがあるため）。
    /// </remarks>
    public sealed class AppSession : IDisposable
    {
        static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(15);

        /// <summary>
        /// コピーしないファイルです（アカウントの情報を含むことがあるもの）。
        /// </summary>
        static readonly string[] excludedFileNames = ["PPD.ini", "credentials.txt", "Data.pak"];

        readonly Application application;

        public UIA3Automation Automation { get; } = new();

        /// <summary>
        /// アプリをコピーした作業のフォルダです。
        /// </summary>
        public string WorkDirectory { get; }

        public Window MainWindow { get; }

        private AppSession(string sourceDirectory, string exeName)
        {
            WorkDirectory = Path.Combine(Path.GetTempPath(), "PPDUITests", Guid.NewGuid().ToString("N"));
            CopyDirectory(sourceDirectory, WorkDirectory);
            application = Application.Launch(new ProcessStartInfo(Path.Combine(WorkDirectory, exeName))
            {
                WorkingDirectory = WorkDirectory
            });
            MainWindow = application.GetMainWindow(Automation, defaultTimeout)
                ?? throw new InvalidOperationException(exeName + " のウィンドウが開きませんでした。");
        }

        /// <summary>
        /// ソリューションの中のアプリ（ビルドしたもの）を起動します。
        /// </summary>
        /// <param name="projectDirectory">ソリューションのフォルダからの、プロジェクトのフォルダ。</param>
        /// <param name="exeName">exe の名前。</param>
        public static AppSession Start(string projectDirectory, string exeName)
        {
            var assembly = typeof(AppSession).Assembly;
            string Metadata(string key) => assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == key).Value;
            var outputDirectory = Path.GetFullPath(Path.Combine(Metadata("SourceRoot"), projectDirectory, "bin", "x64", Metadata("Configuration")));
            if (!File.Exists(Path.Combine(outputDirectory, exeName)))
            {
                throw new FileNotFoundException("テストするアプリがビルドされていません。", Path.Combine(outputDirectory, exeName));
            }
            return new AppSession(outputDirectory, exeName);
        }

        /// <summary>
        /// 指定のタイトルの、このアプリのウィンドウを待って返します。
        /// </summary>
        /// <remarks>
        /// 所有者のあるウィンドウ（Owner を設定したもの）は、UI Automation では所有者のウィンドウの子になるので、そこも探す。
        /// </remarks>
        public Window WaitForWindow(string title)
        {
            var result = Retry.WhileNull(() => application.GetAllTopLevelWindows(Automation)
                .Concat(MainWindow.FindAllChildren(cf => cf.ByControlType(ControlType.Window)).Select(w => w.AsWindow()))
                .FirstOrDefault(w => w.Title == title), defaultTimeout);
            return result.Result ?? throw new TimeoutException("ウィンドウ「" + title + "」が開きませんでした。");
        }

        /// <summary>
        /// メインのウィンドウの上に開いたモーダルのウィンドウ（メッセージボックスなど）を待って返します。
        /// </summary>
        public Window WaitForModal()
        {
            var result = Retry.WhileNull(() => MainWindow.ModalWindows.FirstOrDefault(), defaultTimeout);
            return result.Result ?? throw new TimeoutException("モーダルのウィンドウが開きませんでした。");
        }

        /// <summary>
        /// 条件が満たされるまで待ちます。
        /// </summary>
        public static bool WaitUntil(Func<bool> condition)
        {
            return Retry.WhileFalse(condition, defaultTimeout).Result;
        }

        /// <summary>
        /// ボタンを押します。モーダルのウィンドウを開くボタンは、閉じるまで戻らないことがあるので、別のスレッドで押す。
        /// </summary>
        public static void InvokeAsync(Button button)
        {
            Task.Run(() => button.Invoke());
        }

        public void Dispose()
        {
            try
            {
                if (!application.HasExited)
                {
                    application.Kill();
                }
            }
            catch (InvalidOperationException)
            {
            }
            application.Dispose();
            Automation.Dispose();
            // 終了したばかりのプロセスがファイルを握っていることがあるので、何度か試す
            for (int i = 0; i < 10 && Directory.Exists(WorkDirectory); i++)
            {
                try
                {
                    Directory.Delete(WorkDirectory, true);
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(300);
                }
                catch (UnauthorizedAccessException)
                {
                    System.Threading.Thread.Sleep(300);
                }
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
            {
                if (excludedFileNames.Contains(Path.GetFileName(file), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
            }
            foreach (var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
            }
        }
    }
}
