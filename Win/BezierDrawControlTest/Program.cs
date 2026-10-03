using System;
using System.Windows;

namespace BezierDrawControlTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            new Application().Run(new MainWindow());
        }
    }
}
