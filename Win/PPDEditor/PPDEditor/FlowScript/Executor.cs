using FlowScriptEngine;
using PPDCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PPDEditor.FlowScript
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "止まらなかったスクリプトのスレッドが DebugController を使い続ける可能性があるので破棄しない（イベントのハンドルは GC で解放される）")]
    class Executor
    {
        private readonly Dictionary<string, object> items;
        private Thread thread;
        private volatile bool killed;
        private readonly DebugController debugController;
        private readonly TcpDebugControllerClient controllerClient;

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
            items = [];
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
                // .NET では Thread.Abort が使えないので、スクリプトに中止を要求する（次のノードの評価で止まる）。
                // 止まらない場合（ノードの中で処理が終わらない場合）も、バックグラウンドスレッドなのでアプリの終了は妨げない
                killed = true;
                debugController.Abort();
                thread.Join(3000);
            }
        }

        public void Execute()
        {
            thread = new Thread(ExecuteImpl) { IsBackground = true };
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
                controllerClient?.Close();
            }
            if (killed)
            {
                // 以前は Kill で Thread.Abort していたので、Finished は呼ばれなかった
                return;
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
