using PPDFramework;
using System;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (MyGame game = new MyGame(new PPDExecuteArg(new string[] { })))
            {
                try
                {
                    game.Run();
                }
                catch (Exception e)
                {
                    errorHandler.ProcessError(e);
                    game.Form.MainForm.Close();
                }
            }
        }
    }
}