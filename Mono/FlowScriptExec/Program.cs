using System;
using System.IO;
using FlowScriptEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace FlowScriptExec
{
    class MainClass
    {
        public static string ApplicationDir
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                Console.Error.WriteLine("No Script Provided");
                Environment.ExitCode = -1;
                return;
            }

            EnumerateFile("FlowScriptEngineBasic.dll");
            EnumerateFile("FlowScriptEngineBasicExtension.dll");
            EnumerateFile("FlowScriptEngineConsole.dll");
            EnumerateFile("FlowScriptEngineData.dll");

            var thread = new Thread(Execute);
            thread.Start(args);
            if (!thread.Join(TimeSpan.FromSeconds(5)))
            {
                thread.Abort();
                Console.Error.WriteLine("Time Limit Exceeded");
                Environment.ExitCode = -1;
            }
        }

        private static void EnumerateFile(string fileName)
        {
            foreach (var asmAndType in FlowSourceEnumerator.EnumerateFromFile(Path.Combine(ApplicationDir, fileName), new Type[] { typeof(FlowSourceObjectBase) }))
            {
            }
        }

        private static void Execute(object val)
        {
            string[] args = (string[])val;
            try
            {
                Engine engine = new Engine();
                using (FileStream stream = File.Open(args[0], FileMode.Open))
                {
                    FlowSourceManager manager = engine.Load(stream, false);
                    manager.Initialize();
                    engine.Start();
                }
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.ExitCode = -1;
            }
        }
    }
}