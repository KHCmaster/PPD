using FlowScriptEngine;
using PPDCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PPDEditor.FlowScript
{
    class Executor
    {
        private Dictionary<string, object> items;
        private Thread thread;
        private DebugController debugController;
        private TcpDebugControllerClient controllerClient;

        public string ScriptFilePath
        {
            get;
            private set;
        }

        public bool HasError
        {
            get;
            private set;
        }

        public string ErrorText
        {
            get;
            private set;
        }

        public Layer[] Layers
        {
            get;
            private set;
        }

        public bool IsWorking
        {
            get
            {
                return thread != null && thread.IsAlive;
            }
        }

        public event EventHandler Finished;

        public Executor(string scriptFilePath, Layer[] layers)
        {
            ScriptFilePath = scriptFilePath;
            Layers = layers;
            debugController = new DebugController();
            controllerClient = new TcpDebugControllerClient(debugController);
            items = new Dictionary<string, object>();
            AddItem("AllLayers", layers);
        }

        public void AddItem(string key, object value)
        {
            items[key] = value;
        }

        public void Kill()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                while (!thread.Join(100))
                {
                    thread.Abort();
                }
            }
        }

        public void Execute()
        {
            thread = new Thread(ExecuteImpl);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void ExecuteImpl()
        {
            try
            {
                if (TcpDebugControllerBase.IsListening())
                {
                    controllerClient.Create();
                }
                var engine = new Engine();
                engine.Error += engine_Error;
                using (FileStream fs = File.Open(ScriptFilePath, FileMode.Open))
                {
                    var manager = engine.Load(fs, true, Guid.NewGuid(), debugController);
                    if (controllerClient.BreakPoints.TryGetValue(ScriptFilePath, out int[] breakPoints))
                    {
                        foreach (var id in breakPoints)
                        {
                            manager.AddBreakPoint(id);
                        }
                    }
                    controllerClient.AddFlowSourceManager(ScriptFilePath, manager);
                    foreach (var item in items)
                    {
                        manager.Items[item.Key] = item.Value;
                    }
                    manager.Initialize();
                }
                engine.Start();
            }
            catch (Exception e)
            {
                ProcessError(e);
            }
            finally
            {
                if (controllerClient != null)
                {
                    controllerClient.Close();
                }
            }
            Finished?.Invoke(this, EventArgs.Empty);
        }

        void engine_Error(FlowExecutionException obj)
        {
            ProcessError(obj);
        }

        private void ProcessError(Exception e)
        {
            HasError = true;
            ErrorText += e.ToString();
        }
    }
}
