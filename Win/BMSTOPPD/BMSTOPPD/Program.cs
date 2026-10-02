using System;
using System.Windows;

namespace BMSTOPPD
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 設定のウィンドウは閉じても隠れるだけなので、メインのウィンドウを閉じたら終了する
            var application = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
            application.Run(new MainWindow());
        }
    }
}
