using PPDFramework;
using System;
using System.Windows.Forms;

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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }
    }
}
