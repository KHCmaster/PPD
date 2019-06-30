using ErrorHandle;
using System;
using System.ServiceProcess;

namespace PPDMultiServerService
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main(string[] args)
        {
            var errorHandler = new ErrorHandler();
            errorHandler.Initialize();
            try
            {

                if (args.Length > 0 && args[0] == "standalone")
                {
                    Console.WriteLine("Working as standalone mode");
                    var service = new Service(true);
                    Console.ReadLine();
                    service.Stop();
                    return;
                }

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }
    }
}
