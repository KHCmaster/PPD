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
        /// コピーしないファイルです（アカウントの情報や鍵を含むことがあるもの）。「*.拡張子」は、その拡張子のすべてのファイル。
        /// </summary>
        static readonly string[] excludedFileNames = ["PPD.ini", "credentials.txt", "Data.pak", "*.ppk", "lastUser.txt"];

        readonly Application application;

        public UIA3Automation Automation { get; } = new();

        /// <summary>
        /// テストのプロセスを、モニターごとの DPI に対応させます（UI Automation の座標と、画面を撮る座標・マウスの座標を合わせるため）。
        /// </summary>
        static AppSession()
        {
            SetProcessDpiAwarenessContext(new IntPtr(-4));
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        /// <summary>
        /// アプリをコピーした作業のフォルダです。
        /// </summary>
        public string WorkDirectory { get; }

        public Window MainWindow { get; }

        private AppSession(string sourceDirectory, string exeName, AppStartOptions options)
        {
            WorkDirectory = Path.Combine(Path.GetTempPath(), "PPDUITests", Guid.NewGuid().ToString("N"));
            try
            {
                CopyDirectory(sourceDirectory, WorkDirectory, [.. excludedFileNames, .. options.ExcludedFiles]);
                // 準備や確かめに失敗したら、アプリを起動しない
                options.Prepare?.Invoke(WorkDirectory);
            }
            catch
            {
                DeleteWorkDirectory();
                throw;
            }
            var startInfo = new ProcessStartInfo(Path.Combine(WorkDirectory, exeName))
            {
                WorkingDirectory = WorkDirectory,
                UseShellExecute = false
            };
            foreach (var (name, value) in options.Environment)
            {
                startInfo.Environment[name] = value.Replace("{WorkDirectory}", WorkDirectory);
            }
            application = Application.Launch(startInfo);
            // 最初に開いたウィンドウを待つ。タスクバーに出ないウィンドウ（ShowInTaskbar="False"）は、
            // プロセスのメインのウィンドウにならないので、プロセスのトップレベルのウィンドウから探す
            MainWindow = Retry.WhileNull(() => application.GetAllTopLevelWindows(Automation).FirstOrDefault(), defaultTimeout).Result;
            if (MainWindow == null)
            {
                // 起動に失敗したときも、アプリを残さない（例外を投げるとテストの Dispose は呼ばれない）
                Dispose();
                throw new InvalidOperationException(exeName + " のウィンドウが開きませんでした。");
            }
        }

        /// <summary>
        /// ソリューションの中のアプリ（ビルドしたもの）を起動します。
        /// </summary>
        /// <param name="projectDirectory">ソリューションのフォルダからの、プロジェクトのフォルダ。</param>
        /// <param name="exeName">exe の名前。</param>
        /// <param name="excludedFiles">ほかにコピーしないファイルの名前（ネットワークにつなぐ設定のファイルなど）。</param>
        public static AppSession Start(string projectDirectory, string exeName, params string[] excludedFiles)
        {
            return Start(projectDirectory, exeName, new AppStartOptions { ExcludedFiles = excludedFiles });
        }

        /// <summary>
        /// ソリューションの中のアプリ（ビルドしたもの）を、指定のしかたで起動します。
        /// </summary>
        public static AppSession Start(string projectDirectory, string exeName, AppStartOptions options)
        {
            var assembly = typeof(AppSession).Assembly;
            string Metadata(string key) => assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == key).Value;
            var outputDirectory = Path.GetFullPath(Path.Combine(Metadata("SourceRoot"), projectDirectory, "bin", "x64", Metadata("Configuration")));
            if (!File.Exists(Path.Combine(outputDirectory, exeName)))
            {
                throw new FileNotFoundException("テストするアプリがビルドされていません。", Path.Combine(outputDirectory, exeName));
            }
            return new AppSession(outputDirectory, exeName, options);
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
        /// ウィンドウ（省略したときはメインのウィンドウ）の上に開いたモーダルのウィンドウ（メッセージボックスなど）を待って返します。
        /// </summary>
        public Window WaitForModal(Window owner = null)
        {
            owner ??= MainWindow;
            var result = Retry.WhileNull(() => owner.ModalWindows.FirstOrDefault(), defaultTimeout);
            return result.Result ?? throw new TimeoutException("モーダルのウィンドウが開きませんでした。");
        }

        /// <summary>
        /// メインのウィンドウの中の、AutomationId の要素を返します（表示されるまで待つ）。
        /// </summary>
        public AutomationElement Find(string automationId)
        {
            return Retry.WhileNull(() => MainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId)), defaultTimeout).Result
                ?? throw new InvalidOperationException(automationId + " が見つかりません。");
        }

        /// <summary>
        /// メニューを順に開いて、最後の項目を返します（押さない）。
        /// </summary>
        /// <param name="automationIds">メニューの項目の AutomationId（一番上のメニューから順に）。</param>
        public MenuItem FindMenuItem(params string[] automationIds)
        {
            var item = Find(automationIds[0]).AsMenuItem();
            for (int i = 1; i < automationIds.Length; i++)
            {
                item.Expand();
                var id = automationIds[i];
                item = item.Items.FirstOrDefault(child => child.AutomationId == id)
                    ?? throw new InvalidOperationException("メニューに " + id + " がありません。");
            }
            return item;
        }

        /// <summary>
        /// 条件が満たされるまで待ちます。
        /// </summary>
        public static bool WaitUntil(Func<bool> condition)
        {
            return Retry.WhileFalse(condition, defaultTimeout).Result;
        }

        /// <summary>
        /// ボタンやメニューの項目を押します。モーダルのウィンドウを開くものは、閉じるまで戻らないことがあるので、別のスレッドで押す。
        /// </summary>
        public static void InvokeAsync(AutomationElement element)
        {
            Task.Run(() => element.Patterns.Invoke.Pattern.Invoke());
        }

        /// <summary>
        /// アプリが終わったかどうかです。
        /// </summary>
        public bool HasExited
        {
            get { return application.HasExited; }
        }

        /// <summary>
        /// アプリが終わるまで待ちます。
        /// </summary>
        /// <returns>終わったかどうか。</returns>
        public bool WaitForExit()
        {
            return WaitUntil(() => application.HasExited);
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
            DeleteWorkDirectory();
        }

        private void DeleteWorkDirectory()
        {
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

        private static bool IsExcluded(string fileName, string[] excluded)
        {
            return excluded.Any(pattern => pattern.StartsWith("*.", StringComparison.Ordinal)
                ? fileName.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase)
                : fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private static void CopyDirectory(string source, string destination, string[] excluded)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
            {
                if (IsExcluded(Path.GetFileName(file), excluded))
                {
                    continue;
                }
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
            }
            foreach (var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)), excluded);
            }
        }
    }

    /// <summary>
    /// アプリの起動のしかたです。
    /// </summary>
    public sealed class AppStartOptions
    {
        /// <summary>
        /// ほかにコピーしないファイルの名前です。
        /// </summary>
        public string[] ExcludedFiles { get; init; } = [];

        /// <summary>
        /// 起動する前に、作業のフォルダ（引数）を準備する処理です（テスト用のデータを置く、コピーしてはいけないものがないか確かめるなど）。
        /// 例外を投げると、アプリを起動しない。
        /// </summary>
        public Action<string> Prepare { get; init; }

        /// <summary>
        /// アプリの環境変数です。値の「{WorkDirectory}」は、作業のフォルダのパスにする。
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> Environment { get; init; } = [];
    }
}
