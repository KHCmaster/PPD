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
            try
            {
                // テストが止まって強制終了されても、アプリを残さない
                ChildProcessJob.Add(application.ProcessId);
                // 最初に開いたウィンドウを待つ。タスクバーに出ないウィンドウ（ShowInTaskbar="False"）は、
                // プロセスのメインのウィンドウにならないので、プロセスのトップレベルのウィンドウから探す。
                // 起動の画面（スプラッシュ）を出すアプリは、タイトルでメインのウィンドウを選ぶ
                var handle = Retry.While(() => NativeWindows.OfProcess(application.ProcessId)
                    .FirstOrDefault(h => options.MainWindowTitle == null || NativeWindows.GetTitle(h) == options.MainWindowTitle), h => h == IntPtr.Zero, options.StartupTimeout ?? defaultTimeout).Result;
                if (handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(exeName + " のウィンドウが開きませんでした。");
                }
                MainWindow = Automation.FromHandle(handle).AsWindow();
            }
            catch
            {
                // 起動に失敗したときも、アプリを残さない（例外を投げるとテストの Dispose は呼ばれない）
                Dispose();
                throw;
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
        /// 指定のタイトルの、このアプリのトップレベルのウィンドウ（所有者のあるものも含む）を待って返します。
        /// </summary>
        /// <remarks>
        /// ウィンドウは Win32 で探す（UI Automation で探すと、ゲームのループのように UI のスレッドが忙しいアプリでは時間がかかる）。
        /// </remarks>
        public Window WaitForWindow(string title)
        {
            var result = Retry.While(() => NativeWindows.OfProcess(application.ProcessId).FirstOrDefault(h => NativeWindows.GetTitle(h) == title), h => h == IntPtr.Zero, defaultTimeout);
            if (result.Result == IntPtr.Zero)
            {
                throw new TimeoutException("ウィンドウ「" + title + "」が開きませんでした。");
            }
            return Automation.FromHandle(result.Result).AsWindow();
        }

        /// <summary>
        /// ウィンドウ（省略したときはメインのウィンドウ）の上に開いたモーダルのウィンドウ（メッセージボックスなど）を待って返します。
        /// </summary>
        public Window WaitForModal(Window owner = null)
        {
            owner ??= MainWindow;
            var result = Retry.WhileNull(() => ModalWindowsOf(owner).FirstOrDefault(), defaultTimeout);
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
        /// ウィンドウ（省略したときはメインのウィンドウ）の上に開いているモーダルのウィンドウです。
        /// </summary>
        /// <remarks>
        /// モーダルのウィンドウ（ダイアログやメッセージボックス）を開くと、所有者のウィンドウは使えなくなる。
        /// そのときの、所有者のウィンドウが持つ見えているウィンドウを返す。ウィンドウは Win32 で探す（速いため）。
        /// </remarks>
        public Window[] ModalWindowsOf(Window owner = null)
        {
            var ownerHandle = HandleOf(owner ?? MainWindow);
            if (NativeWindows.IsEnabled(ownerHandle))
            {
                return [];
            }
            return [.. NativeWindows.OfProcess(application.ProcessId)
                .Where(h => NativeWindows.GetOwner(h) == ownerHandle && NativeWindows.HasCaption(h))
                .Select(h => Automation.FromHandle(h).AsWindow())];
        }

        /// <summary>
        /// ウィンドウのハンドルです（UI Automation に聞くのは 1 回だけにする）。
        /// </summary>
        private IntPtr HandleOf(Window window)
        {
            if (!handles.TryGetValue(window, out var handle))
            {
                handle = window.Properties.NativeWindowHandle.Value;
                handles[window] = handle;
            }
            return handle;
        }

        readonly System.Collections.Generic.Dictionary<Window, IntPtr> handles = [];

        /// <summary>
        /// 要素の左上からの位置（WPF の単位。DPI で倍率を掛ける前の値）を、マウスでクリックします。
        /// 描いて表示するもの（タイムラインなど）の中をクリックするときに使う。マウスを実際に動かす。
        /// </summary>
        public void ClickAt(AutomationElement element, double x, double y, bool rightButton = false)
        {
            MainWindow.SetForeground();
            var scale = NativeWindows.GetScale(HandleOf(MainWindow));
            // 画面の更新の直後は、UI Automation が一時的に COM のエラーを返すことがあるので、読み直す
            var bounds = Retry.WhileException(() => element.BoundingRectangle, TimeSpan.FromSeconds(5), null, throwOnTimeout: true).Result;
            FlaUI.Core.Input.Mouse.Click(new System.Drawing.Point(bounds.X + (int)(x * scale), bounds.Y + (int)(y * scale)),
                rightButton ? FlaUI.Core.Input.MouseButton.Right : FlaUI.Core.Input.MouseButton.Left);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
        }

        /// <summary>
        /// 要素の左上からの位置（WPF の単位）を、マウスでダブルクリックします。
        /// </summary>
        public void DoubleClickAt(AutomationElement element, double x, double y)
        {
            MainWindow.SetForeground();
            var scale = NativeWindows.GetScale(HandleOf(MainWindow));
            var bounds = Retry.WhileException(() => element.BoundingRectangle, TimeSpan.FromSeconds(5), null, throwOnTimeout: true).Result;
            FlaUI.Core.Input.Mouse.DoubleClick(new System.Drawing.Point(bounds.X + (int)(x * scale), bounds.Y + (int)(y * scale)));
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
        }

        /// <summary>
        /// ウィンドウを閉じます（閉じるボタンと同じ。ダイアログではキャンセルになる）。
        /// </summary>
        public void CloseWindow(Window window)
        {
            NativeWindows.Close(HandleOf(window));
        }

        /// <summary>
        /// 開いているモーダルのウィンドウのタイトルです（テストの失敗のメッセージ用）。
        /// </summary>
        public string DescribeModalWindows(Window owner = null)
        {
            var ownerHandle = HandleOf(owner ?? MainWindow);
            return string.Join(", ", NativeWindows.OfProcess(application.ProcessId)
                .Where(h => NativeWindows.GetOwner(h) == ownerHandle)
                .Select(h => "「" + NativeWindows.GetTitle(h) + "」" + (NativeWindows.HasCaption(h) ? "" : "(タイトルバーなし)")));
        }

        /// <summary>
        /// モーダルのウィンドウがすべて閉じるまで待ちます。
        /// </summary>
        /// <returns>閉じたかどうか。</returns>
        public bool WaitForNoModal(Window owner = null)
        {
            return WaitUntil(() => ModalWindowsOf(owner).Length == 0);
        }

        /// <summary>
        /// 要素を右クリックして、出たメニューの中の AutomationId の要素を返します。メニューが出なければ、3 回まで右クリックし直します
        /// （画面の更新の直後は、右クリックしてもメニューが出ないことがある）。
        /// </summary>
        public AutomationElement RightClickForPopup(AutomationElement element, double x, double y, string automationId)
        {
            for (var i = 0; i < 3; i++)
            {
                ClickAt(element, x, y, rightButton: true);
                try
                {
                    return FindInPopups(automationId, TimeSpan.FromSeconds(3));
                }
                catch (InvalidOperationException)
                {
                }
            }
            throw new InvalidOperationException(automationId + " がポップアップに出ません。");
        }

        /// <summary>
        /// ポップアップ（右クリックのメニューなど、このアプリの別のウィンドウ）の中の、AutomationId の要素を待って返します。
        /// </summary>
        public AutomationElement FindInPopups(string automationId, TimeSpan? timeout = null)
        {
            var result = Retry.WhileNull(() => Automation.GetDesktop()
                .FindAllChildren(cf => cf.ByProcessId(application.ProcessId))
                .Select(window => window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)))
                .FirstOrDefault(element => element != null), timeout ?? defaultTimeout);
            return result.Result ?? throw new InvalidOperationException(automationId + " がポップアップにありません。");
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
                if (IsExcluded(Path.GetFileName(directory), excluded))
                {
                    continue;
                }
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
        /// ほかにコピーしないファイルやフォルダの名前です。
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

        /// <summary>
        /// メインのウィンドウのタイトルです。省略したときは、最初に開いたウィンドウをメインのウィンドウにする。
        /// </summary>
        public string MainWindowTitle { get; init; }

        /// <summary>
        /// メインのウィンドウが開くまで待つ時間です。省略したときは既定の時間。
        /// </summary>
        public TimeSpan? StartupTimeout { get; init; }
    }
}
