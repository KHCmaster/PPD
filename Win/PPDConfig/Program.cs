using PPDConfig.ViewModels;
using PPDFramework;
using System;
using System.Windows;

namespace PPDConfig
{
    static class Program
    {
        public static ErrorHandlerPPD ErrorHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ErrorHandler = new ErrorHandlerPPD();
            ErrorHandler.Initialize();

            var application = new Application();
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.DataContext = new MainWindowViewModel(mainWindow, "PPD.ini", "PPDConfig.ini");
                application.Run(mainWindow);
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }
    }
}
