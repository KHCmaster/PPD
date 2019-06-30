using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PPDCore
{
    public class TcpDebugControllerHost : TcpDebugControllerBase
    {
        private TcpListener listener;
        private TcpClient client;
        private Thread thread;
        private Dictionary<string, int[]> infos;

        public event Action<string, string> SourceChanged;
        public event Action<string> OperationWaited;
        public event Action<string> OperationAccepted;
        public event Action<string, int, string> ErrorOccurred;
        public event Action<string, string> LogReceived;

        public TcpDebugControllerHost(Dictionary<string, int[]> infos)
        {
            this.infos = infos;
        }

        public void Create()
        {
            listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();
            IAsyncResult result = null;
            result = listener.BeginAcceptTcpClient(state =>
            {
                if (listener == null)
                {
                    return;
                }
                client = listener.EndAcceptTcpClient(result);
                thread = ThreadManager.Instance.GetThread(Work);
                thread.Start();
            }, listener);
        }

        public void Close()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                thread = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
        }

        public void Continue()
        {
            var bytes = GetBytes(new string[] { MethodName }, writer =>
            {
                WriteString(writer, ContinueMethod);
            });
            Send(bytes);
        }

        public void StepIn()
        {
            var bytes = GetBytes(new string[] { MethodName }, writer =>
            {
                WriteString(writer, StepInMethod);
            });
            Send(bytes);
        }

        public void AddBreakPoint(string fileName, int sourceId)
        {
            var bytes = GetBytes(new string[] { MethodName, SourceId, FileName }, writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, AddBreakPointMethod);
                        break;
                    case SourceId:
                        WriteInt(writer, sourceId);
                        break;
                    case FileName:
                        WriteString(writer, fileName);
                        break;
                }
            });
            Send(bytes);
        }

        public void RemoveBreakPoint(string fileName, int sourceId)
        {
            var bytes = GetBytes(new string[] { MethodName, SourceId, FileName }, writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, RemoveBreakPointMethod);
                        break;
                    case SourceId:
                        WriteInt(writer, sourceId);
                        break;
                    case FileName:
                        WriteString(writer, fileName);
                        break;
                }
            });
            Send(bytes);
        }

        public void ChangeSource(string fileName, int sourceId)
        {
            var bytes = GetBytes(new string[] { MethodName, SourceId, FileName }, writer =>
            {
                switch (writer.Filename)
                {
                    case MethodName:
                        WriteString(writer, ChangeSourceMethod);
                        break;
                    case SourceId:
                        WriteInt(writer, sourceId);
                        break;
                    case FileName:
                        WriteString(writer, fileName);
                        break;
                }
            });
            Send(bytes);
        }

        public void Work()
        {
            byte[] lastBytes = new byte[0];
            while (true)
            {
                var bytes = Read(client.GetStream());
                if (bytes.Length == 0)
                {
                    return;
                }
                bytes = lastBytes.Concat(bytes).ToArray();
                while (true)
                {
                    var lastLength = bytes.Length;
                    var restBytes = WorkImpl(bytes);
                    if (restBytes.Length == 0 || restBytes.Length == lastLength)
                    {
                        lastBytes = restBytes;
                        break;
                    }
                    bytes = restBytes;
                }
            }
        }

        private byte[] WorkImpl(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                using (PackReader reader = new PackReader(stream))
                {
                    var now = DateTime.Now;
                    var methodName = ParseString(reader, MethodName);
                    switch (methodName)
                    {
                        case GetFileList:
                            var sendBytes = GetBytes(new string[] { Return }, writer =>
                            {
                                var fileList = infos.Keys.Aggregate("", (s1, s2) => String.IsNullOrEmpty(s1) ? s2 : String.Format("{0},{1}", s1, s2));
                                WriteString(writer, fileList);
                            });
                            Send(sendBytes);
                            break;
                        case GetBreakPoints:
                            var fileName = ParseString(reader, FileName);
                            int[] breakPoints;
                            if (!infos.TryGetValue(fileName, out breakPoints))
                            {
                                breakPoints = new int[0];
                            }
                            sendBytes = GetBytes(new string[] { Return }, writer =>
                            {
                                WriteString(writer, breakPoints.Aggregate("", (s, id) => String.IsNullOrEmpty(s) ? id.ToString() : String.Format("{0},{1}", s, id)));
                            });
                            Send(sendBytes);
                            break;
                        case SourceChangedEvent:
                            var serializedText = ParseString(reader, SerializedText);
                            fileName = ParseString(reader, FileName);
                            OnSourceChanged(serializedText, fileName);
                            break;
                        case OperationAcceptedEvent:
                            fileName = ParseString(reader, FileName);
                            OnOperationAccepted(fileName);
                            break;
                        case OperationWaitedEvent:
                            fileName = ParseString(reader, FileName);
                            OnOperationWaited(fileName);
                            break;
                        case ErrorOccurredEvent:
                            fileName = ParseString(reader, FileName);
                            var errorText = ParseString(reader, ErrorText);
                            var errorId = ParseInt(reader, ErrorId);
                            OnErrorOccurred(fileName, errorId, errorText);
                            break;
                        case LogReceivedEvent:
                            var logType = ParseString(reader, LogType);
                            var logText = ParseString(reader, LogText);
                            OnLogReceived(logType, logText);
                            break;
                    }

                    byte[] restBytes = new byte[bytes.Length - reader.LastIndex];
                    if (restBytes.Length > 0)
                    {
                        Array.Copy(bytes, reader.LastIndex, restBytes, 0, restBytes.Length);
                    }
                    return restBytes;
                }
            }
            catch
            {
                return bytes;
            }
        }

        private void Send(byte[] bytes)
        {
            client.GetStream().Write(bytes, 0, bytes.Length);
        }

        private void OnSourceChanged(string serializedText, string fileName)
        {
            SourceChanged?.Invoke(serializedText, fileName);
        }

        private void OnOperationAccepted(string fileName)
        {
            OperationAccepted?.Invoke(fileName);
        }

        private void OnOperationWaited(string fileName)
        {
            OperationWaited?.Invoke(fileName);
        }

        private void OnErrorOccurred(string fileName, int id, string error)
        {
            ErrorOccurred?.Invoke(fileName, id, error);
        }

        private void OnLogReceived(string logType, string logText)
        {
            LogReceived?.Invoke(logType, logText);
        }
    }
}
