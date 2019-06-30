using System;
using System.Windows.Forms;

namespace PPDTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (MyGame game = new MyGame(new PPDFramework.PPDExecuteArg(new string[0])))
            {
                game.Run();
            }
        }
    }
}
