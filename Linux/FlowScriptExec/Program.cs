using System;
using System.IO;
using FlowScriptEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;

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

            // .NET では Shift_JIS（932）などのコードページが既定では使えないので登録する（FlowScript の EncodingType で使う）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            EnumerateFile("FlowScriptEngineBasic.dll");
            EnumerateFile("FlowScriptEngineBasicExtension.dll");
            EnumerateFile("FlowScriptEngineConsole.dll");
            EnumerateFile("FlowScriptEngineData.dll");

            // .NET では Thread.Abort が使えないので、時間切れのときはプロセスごと終了する
            var thread = new Thread(Execute)
            {
                IsBackground = true
            };
            thread.Start(args);
            if (!thread.Join(TimeSpan.FromSeconds(5)))
            {
                Console.Error.WriteLine("Time Limit Exceeded");
                Environment.Exit(-1);
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