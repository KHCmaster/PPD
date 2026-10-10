using FlowScriptControlTest.Models;
using FlowScriptEngine;
using System;
using System.IO;
using System.Windows;

namespace FlowScriptControlTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。引数にスクリプトのファイルを渡すと、ウィンドウを出さずに実行します。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                FlowEnvironment.InitializeForEditing();
                var application = new Application();
                application.Run(new MainWindow());
            }
            else
            {
                if (!File.Exists(args[0]))
                {
                    return;
                }

                FlowEnvironment.LoadClasses();
                var engine = new Engine();
                try
                {
                    using FileStream stream = File.Open(args[0], FileMode.Open);
                    var manager = engine.Load(stream, false);
                    manager.Initialize();
                    engine.Start();
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
