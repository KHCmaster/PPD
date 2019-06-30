using FlowScriptControl.Controls;
using FlowScriptEngine;
using System;
using System.IO;
using System.Windows.Forms;

namespace FlowScriptControlTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                if (!File.Exists(args[0]))
                {
                    return;
                }

                FlowDrawPanel.EnumerateClasses("dlls", new string[0]);
                var engine = new Engine();
                try
                {
                    using (FileStream stream = File.Open(args[0], FileMode.Open))
                    {
                        var manager = engine.Load(stream, false);
                        manager.Initialize();
                        engine.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.Write(ex);
                }
                Console.ReadLine();
            }
        }
    }
}
