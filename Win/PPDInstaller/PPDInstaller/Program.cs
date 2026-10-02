using ErrorHandle;
using System;
using System.Windows;

namespace PPDInstaller
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var errorHandler = new ErrorHandler();
            errorHandler.Initialize();
            System.Environment.CurrentDirectory = AppContext.BaseDirectory;
            try
            {
                var application = new Application
                {
                    // メインウィンドウの前に言語の選択と展開のウィンドウを出すので、それらを閉じても終了しないようにする
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                var window = new MainWindow();
                application.ShutdownMode = ShutdownMode.OnMainWindowClose;
                application.Run(window);
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }
    }
}
