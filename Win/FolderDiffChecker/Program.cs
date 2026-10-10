using System;
using System.Windows;

namespace FolderDiffChecker
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var application = new Application();
            application.Run(new MainWindow());
        }
    }
}
