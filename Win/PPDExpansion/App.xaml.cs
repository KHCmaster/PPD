using ErrorHandle;
using System;
using System.Runtime.ExceptionServices;
using System.Windows;

namespace PPDExpansion
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static int Port
        {
            get;
            private set;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.FirstChanceException +=
           (object source, FirstChanceExceptionEventArgs ee) =>
           {
               Console.WriteLine("FirstChanceException event raised in {0}: {1}",
                   AppDomain.CurrentDomain.FriendlyName, ee.Exception.Message);
           };
            var errorHandler = new ErrorHandler();
            errorHandler.Initialize();
            if (e.Args.Length == 0)
            {
                Console.WriteLine("Run with port");
                Environment.Exit(-1);
                return;
            }
            if (!int.TryParse(e.Args[0], out int val))
            {
                Console.WriteLine("Port is not integer");
                Environment.Exit(-1);
            }
            if (val < 0 || val > 65535)
            {
                Console.WriteLine("Port is out of range");
                Environment.Exit(-1);
            }
            Port = val;

            base.OnStartup(e);
        }
    }
}
