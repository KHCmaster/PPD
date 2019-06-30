using ErrorHandle;
using System;
using System.IO;
using System.Windows.Forms;

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
            System.Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
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