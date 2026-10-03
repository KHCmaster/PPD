using ErrorHandle;
using System;
using System.Windows;

namespace Effect2DEditor
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
            try
            {
                new Application().Run(new MainWindow());
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }
    }
}
