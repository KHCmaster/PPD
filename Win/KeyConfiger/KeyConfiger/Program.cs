using KeyConfiger.ViewModels;
using PPDFramework;
using System;
using System.Windows;

namespace KeyConfiger
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var errorHandler = new ErrorHandlerPPD();
            errorHandler.Initialize();
            var application = new Application();
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.DataContext = new MainWindowViewModel(mainWindow, "keyconfig.ini", "KeyConfiger.ini");
                application.Run(mainWindow);
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }
    }
}
