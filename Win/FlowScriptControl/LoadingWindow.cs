using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FlowScriptControl
{
    /// <summary>
    /// スクリプトの読み込みの進み具合を表示するウィンドウです。
    /// </summary>
    /// <remarks>
    /// 読み込みは呼び出し元（UI）のスレッドで行うので、ウィンドウは別のスレッド（専用の Dispatcher）で表示する。
    /// </remarks>
    static class LoadingWindow
    {
        private static readonly Lock syncObject = new();
        private static Window window;
        private static ProgressBar progressBar;
        private static Dispatcher dispatcher;

        public static void ShowLoading()
        {
            lock (syncObject)
            {
                if (dispatcher != null)
                {
                    return;
                }
                using var ready = new ManualResetEventSlim();
                var thread = new Thread(() =>
                {
                    progressBar = new ProgressBar { Height = 23, Margin = new Thickness(12), Maximum = 100 };
                    window = new Window
                    {
                        Title = " ",
                        Content = progressBar,
                        Width = 350,
                        SizeToContent = SizeToContent.Height,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.ToolWindow,
#if !DEBUG
                        Topmost = true,
#endif
                    };
                    dispatcher = Dispatcher.CurrentDispatcher;
                    window.Show();
                    ready.Set();
                    Dispatcher.Run();
                })
                {
                    Name = "LoadingWindow",
                    IsBackground = true,
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                ready.Wait();
            }
        }

        public static void ChangeProgress(float progress)
        {
            lock (syncObject)
            {
                dispatcher?.BeginInvoke(() =>
                {
                    progressBar?.Value = 100 * progress;
                });
            }
        }

        public static void CloseLoading()
        {
            lock (syncObject)
            {
                if (dispatcher == null)
                {
                    return;
                }
                var closingDispatcher = dispatcher;
                closingDispatcher.BeginInvoke(() =>
                {
                    window?.Close();
                    window = null;
                    progressBar = null;
                    closingDispatcher.InvokeShutdown();
                });
                dispatcher = null;
            }
        }
    }
}
