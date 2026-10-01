using FlowScriptEngine;
using PPDFramework.Logger;
using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace PPDCore
{
    public class TcpDebugControllerClient : TcpDebugControllerBase, IDisposable
    {
        private readonly DebugController debugController;
        private TcpClient tcpClient;
        private bool connected;
        private readonly Dictionary<string, int[]> breakPoints;
        private readonly Dictionary<string, FlowSourceManager> dict;
        private Thread asyncReadThread;

        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        public Dictionary<string, int[]> BreakPoints
        {
            get
            {
                return breakPoints;
            }
        }

        public TcpDebugControllerClient(DebugController debugController)
        {
            this.debugController = debugController;
            breakPoints = [];
            dict = [];

            debugController.OperationWaited += DebugController_OperationWaited;
            debugController.OperationAccepted += DebugController_OperationAccepted;
            debugController.SourceChanged += DebugController_SourceChanged;
            debugController.ErrorOccurred += DebugController_ErrorOccurred;
            LogManager.Instance.LogReceived += Instance_LogReceived;
        }

        private void Instance_LogReceived(LogInfo obj)
        {
            var bytes = GetBytes([MethodName, LogType, LogText], writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, LogReceivedEvent);
                        break;
                    case LogType:
                        WriteString(writer, obj.LogLevel.ToString());
                        break;
                    case LogText:
                        WriteString(writer, obj.Message);
                        break;
                }
            });
            Send(bytes);
        }

        public void AddFlowSourceManager(string fileName, FlowSourceManager flowSourceManager)
        {
            dict.Add(fileName, flowSourceManager);
        }

        public void Create()
        {
            try
            {
                tcpClient = new TcpClient(LocalHost, Port);

                var bytes = GetBytes([MethodName], writer =>
                {
                    switch (writer.Filename)
                    {
                        case MethodName:
                            WriteString(writer, GetFileList);
                            break;
                    }
                });
                Send(bytes);
                bytes = Read(tcpClient.GetStream());
                string[] fileNames;
                using (MemoryStream stream = new(bytes))
                using (PackReader reader = new(stream))
                {
                    var ret = ParseString(reader, Return);
                    fileNames = ret.Split(',');
                }

                foreach (var fileName in fileNames)
                {
                    bytes = GetBytes([MethodName, FileName], writer =>
                    {
                        switch (writer.Filename)
                        {
                            case MethodName:
                                WriteString(writer, GetBreakPoints);
                                break;
                            case FileName:
                                WriteString(writer, fileName);
                                break;
                        }
                    });
                    Send(bytes);
                    bytes = Read(tcpClient.GetStream());
                    using MemoryStream stream = new(bytes);
                    using PackReader reader = new(stream);
                    var ret = ParseString(reader, Return);
                    if (!String.IsNullOrEmpty(ret))
                    {
                        breakPoints.Add(fileName, [.. ret.Split(',').Select(s => int.Parse(s))]);
                    }
                }

                connected = true;
            }
            catch
            {

            }
        }

        public void Close()
        {
            LogManager.Instance.LogReceived -= Instance_LogReceived;
            // .NET では Thread.Abort が使えない。受信のスレッドは、接続を閉じると Read が終わるので止まる
            asyncReadThread = null;

            if (!connected)
            {
                return;
            }


            tcpClient.Close();
            connected = false;
        }

        private string GetFileName(FlowSourceManager flowSourceManager)
        {
            return dict.FirstOrDefault(kvp => kvp.Value == flowSourceManager).Key;
        }

        void DebugController_ErrorOccurred(FlowSourceManager arg1, FlowExecutionException arg2)
        {
            var bytes = GetBytes([MethodName, FileName, ErrorText, ErrorId], writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, ErrorOccurredEvent);
                        break;
                    case FileName:
                        WriteString(writer, GetFileName(arg1));
                        break;
                    case ErrorText:
                        WriteString(writer, arg2.ToString());
                        break;
                    case ErrorId:
                        WriteInt(writer, arg2.SourceObject != null ? arg2.SourceObject.Id : -1);
                        break;
                }
            });
            Send(bytes);
        }

        void DebugController_SourceChanged(string arg1, FlowSourceManager arg2)
        {
            var bytes = GetBytes([MethodName, SerializedText, FileName], writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, SourceChangedEvent);
                        break;
                    case SerializedText:
                        WriteString(writer, arg1);
                        break;
                    case FileName:
                        WriteString(writer, GetFileName(arg2));
                        break;
                }
            });
            Send(bytes);
        }

        void DebugController_OperationAccepted(FlowSourceManager obj)
        {
            var bytes = GetBytes([MethodName, FileName], writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, OperationAcceptedEvent);
                        break;
                    case FileName:
                        WriteString(writer, GetFileName(obj));
                        break;
                }
            });
            Send(bytes);
        }

        void DebugController_OperationWaited(FlowSourceManager obj)
        {
            var bytes = GetBytes([MethodName, FileName], writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, OperationWaitedEvent);
                        break;
                    case FileName:
                        WriteString(writer, GetFileName(obj));
                        break;
                }
            });
            Send(bytes);
            AsyncRead(b =>
            {
                using MemoryStream stream = new(b);
                using PackReader reader = new(stream);
                var methodName = ParseString(reader, MethodName);
                switch (methodName)
                {
                    case ContinueMethod:
                        debugController.Continue();
                        return true;
                    case StepInMethod:
                        debugController.StepIn();
                        return true;
                    case AddBreakPointMethod:
                        var fileName = ParseString(reader, FileName);
                        FlowSourceManager manager;
                        if (dict.TryGetValue(fileName, out manager))
                        {
                            var id = ParseInt(reader, SourceId);
                            manager.AddBreakPoint(id);
                        }
                        break;
                    case RemoveBreakPointMethod:
                        fileName = ParseString(reader, FileName);
                        if (dict.TryGetValue(fileName, out manager))
                        {
                            var id = ParseInt(reader, SourceId);
                            manager.RemoveBreakPoint(id);
                        }
                        break;
                    case ChangeSourceMethod:
                        fileName = ParseString(reader, FileName);
                        if (dict.TryGetValue(fileName, out manager))
                        {
                            var id = ParseInt(reader, SourceId);
                            debugController.ChangeSource(id);
                        }
                        break;
                }
                return false;
            });
        }

        private void Send(byte[] bytes)
        {
            tcpClient.GetStream().Write(bytes, 0, bytes.Length);
        }

        private void AsyncRead(Func<byte[], bool> callback)
        {
            var stream = tcpClient.GetStream();
            asyncReadThread = ThreadManager.Instance.GetThread(() =>
            {
                while (true)
                {
                    var bytes = Read(stream);
                    if (bytes.Length == 0)
                    {
                        // 接続が閉じられた（以前は OnClosed → Close の中でこのスレッドを Abort していた）
                        break;
                    }
                    if (callback(bytes))
                    {
                        break;
                    }
                }
            });
            asyncReadThread.Start();
        }

        protected override void OnClosed()
        {
            connected = false;
            Close();
        }

        /// <summary>
        /// 接続を閉じます（Close() と同じ）。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        /// <param name="disposing">Dispose から呼ばれたときは true。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
    }
}
