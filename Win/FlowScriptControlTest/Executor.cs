using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace FlowScriptControlTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "止まらなかったスクリプトのスレッドが DebugController を使い続ける可能性があるので破棄しない（イベントのハンドルは GC で解放される）")]
    class Executor(ExecuteInfo[] executeInfos, string stdinText)
    {
        public event Action Aborted;
        public event Action Finished;
        private readonly ExecuteInfo[] executeInfos = executeInfos;
        private readonly string stdinText = stdinText;
        private readonly DebugController controller = new DebugController();
        private Thread thread;
        private Thread workingThread;
        private readonly Dictionary<FlowSourceManager, ExecuteInfo> dict = [];

        public DebugController Controller
        {
            get
            {
                return controller;
            }
        }

        public bool IsExecuting
        {
            get
            {
                return workingThread != null && workingThread.IsAlive;
            }
        }

        public ExecuteInfo GetExecuteInfo(FlowSourceManager manager)
        {
            dict.TryGetValue(manager, out ExecuteInfo ret);
            return ret;
        }

        public void Execute()
        {
            thread = new Thread(() =>
            {
                workingThread = new Thread(ExecuteImpl) { IsBackground = true };
                workingThread.Start();
                int executionTime = 0;
                while (workingThread != null && workingThread.IsAlive)
                {
                    if (executionTime > 60000)
                    {
                        // .NET では Thread.Abort が使えないので、スクリプトに中止を要求して少し待つ
                        controller.Abort();
                        workingThread.Join(1000);
                        workingThread = null;
                        OnAborted();
                        break;
                    }
                    Thread.Sleep(100);
                    if (!Controller.Waiting)
                    {
                        executionTime += 100;
                    }
                }
                OnFinished();
            })
            {
                IsBackground = true
            };
            thread.Start();
        }

        public void Abort()
        {
            if (workingThread != null && workingThread.IsAlive)
            {
                controller.Abort();
                if (!workingThread.Join(1000))
                {
                    // 止まらなかった場合も、バックグラウンドスレッドなのでアプリの終了は妨げない
                    workingThread = null;
                }
            }
        }

        private void OnAborted()
        {
            Aborted?.Invoke();
        }

        private void OnFinished()
        {
            Finished?.Invoke();
        }

        private void ExecuteImpl()
        {
            try
            {
                Engine engine = null;
                if (!String.IsNullOrEmpty(stdinText))
                {
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(stdinText));
                    var textReader = new StreamReader(stream);
                    engine = new Engine(textReader);
                }
                else
                {
                    engine = new Engine();
                }
                var guid = Guid.NewGuid();
                foreach (ExecuteInfo executeInfo in executeInfos)
                {
                    var manager = engine.Load(executeInfo.Stream, true, guid, controller);
                    manager.Initialize();
                    foreach (int id in executeInfo.BreakPoints)
                    {
                        manager.AddBreakPoint(id);
                    }
                    executeInfo.BreakPointAdded += (sender, id) =>
                    {
                        manager.AddBreakPoint(id);
                    };
                    executeInfo.BreakPointRemoved += (sender, id) =>
                    {
                        manager.RemoveBreakPoint(id);
                    };
                    dict.Add(manager, executeInfo);
                }
                engine.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Write(ex.StackTrace);
            }
            finally
            {
                foreach (ExecuteInfo executeInfo in executeInfos)
                {
                    executeInfo.RemoveEventHandler();
                }
            }
        }
    }
}
