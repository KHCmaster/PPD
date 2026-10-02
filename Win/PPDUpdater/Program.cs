using System;
using System.Windows;

namespace PPDUpdater
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var errorHandler = new ErrorHandler();
            errorHandler.Initialize();
            var mutex = new System.Threading.Mutex(false, "PPDUpdater");
            if (mutex.WaitOne(0, false) == false)
            {
                return;
            }

            try
            {
                var app = new Application();
                errorHandler.Attach(app);
                var window = new MainWindow();

                WorkState ws = WorkState.None;
                if (Array.IndexOf(args, "-d") != -1) ws |= WorkState.DownLoad;
                if (Array.IndexOf(args, "-i") != -1) ws |= WorkState.Install;

                if (ws != WorkState.None)
                {
                    window.WorkState = ws;
                }
                app.Run(window);
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
            mutex.ReleaseMutex();
            mutex.Close();
        }
    }
}
