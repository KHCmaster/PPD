using ErrorHandle;
using System;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }
    }
}