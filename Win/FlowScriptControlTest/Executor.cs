using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace FlowScriptControlTest
{
    class Executor
    {
        public event Action Aborted;
        public event Action Finished;
        private ExecuteInfo[] executeInfos;
        private string stdinText;
        private DebugController controller;
        private Thread thread;
        private Thread workingThread;
        private Dictionary<FlowSourceManager, ExecuteInfo> dict;

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

        public Executor(ExecuteInfo[] executeInfos, string stdinText)
        {
            this.executeInfos = executeInfos;
            this.stdinText = stdinText;
            controller = new DebugController();
            dict = new Dictionary<FlowSourceManager, ExecuteInfo>();
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
                workingThread = new Thread(ExecuteImpl);
                workingThread.Start();
                int executionTime = 0;
                while (workingThread != null && workingThread.IsAlive)
                {
                    if (executionTime > 60000)
                    {
                        workingThread.Abort();
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
            });
            thread.Start();
        }

        public void Abort()
        {
            if (workingThread != null && workingThread.IsAlive)
            {
                controller.Abort();
                if (!workingThread.Join(100))
                {
                    workingThread.Abort();
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
