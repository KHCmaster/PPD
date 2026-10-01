using System;
using System.Windows.Forms;

namespace PPDTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        /// <param name="args">-scene &lt;番号またはクラス名&gt; で、最初に表示するシーンを指定できます（例: -scene 8、-scene Effect2DScene）。</param>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using MyGame game = new(new PPDFramework.PPDExecuteArg(args));
            game.Run();
        }
    }
}
