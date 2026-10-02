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
                application.Run(new MainWindow());
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }
    }
}
