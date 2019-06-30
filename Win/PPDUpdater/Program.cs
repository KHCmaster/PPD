using System;
using System.Windows.Forms;

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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new Form1();

            WorkState ws = WorkState.None;
            if (Array.IndexOf(args, "-d") != -1) ws |= WorkState.DownLoad;
            if (Array.IndexOf(args, "-i") != -1) ws |= WorkState.Install;

            if (ws != WorkState.None)
            {
                form.WorkState = ws;
            }
            try
            {
                Application.Run(form);
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