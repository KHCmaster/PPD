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
                application.Run(new MainWindow());
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }
    }
}
