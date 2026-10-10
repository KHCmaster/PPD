using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace PPDMultiCommon.Tcp
{
    public abstract class ConnectionBase(TcpClient client, NetworkStream net)
    {
        private readonly Lock syncRoot = new();
        protected TcpClient client = client;
        NetworkStream net = net;
        Thread readThread;
        Thread writeThread;
        readonly Queue<byte[]> jobs = new();

        volatile bool disposed;

        public bool IsClosed
        {
            get;
            protected set;
        }

        public DateTime LastDataReceivedTime
        {
            get;
            protected set;
        } = DateTime.Now;

        ~ConnectionBase()
        {
            Dispose();
        }

        public void Start()
        {
            readThread = ThreadManager.Instance.GetThread(new ThreadStart(ReadThread));
            readThread.IsBackground = true;
            readThread.Start();
            writeThread = ThreadManager.Instance.GetThread(new ThreadStart(WriteThread));
            writeThread.IsBackground = true;
            writeThread.Start();
        }

        private void ReadThread()
        {
            byte[] resBytes = new byte[client.ReceiveBufferSize];
            while (true)
            {
                using MemoryStream ms = new();
                int resSize;
                try
                {
                    do
                    {
                        resSize = net.Read(resBytes, 0, resBytes.Length);
                        if (resSize == 0)
                        {
                            IsClosed = true;
                            return;
                        }
                        ms.Write(resBytes, 0, resSize);
                    } while (net.DataAvailable);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch
                {
                    IsClosed = true;
                    return;
                }

                if (IsClosed)
                {
                    return;
                }

                if (ms.Length > 0)
                {
                    OnRead(ms.ToArray());
                    LastDataReceivedTime = DateTime.Now;
                }
            }
        }

        private void WriteThread()
        {
            while (true)
            {
                if (disposed)
                {
                    // Dispose で止められた（以前は Thread.Abort で止めていたので、OnClose は呼ばない）
                    return;
                }
                if (!IsClosed && jobs.Count > 0)
                {
                    try
                    {
                        if (net.CanWrite)
                        {
                            byte[] sendData;
                            lock (syncRoot)
                            {
                                sendData = jobs.Dequeue();
                            }

                            net.Write(sendData, 0, sendData.Length);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch
                    {
                        IsClosed = true;
                        break;
                    }
                }
                else if (jobs.Count == 0)
                {
                    Thread.Sleep(1);
                }

                if (IsClosed)
                {
                    break;
                }
            }

            if (disposed)
            {
                return;
            }
            OnClose();
        }

        public void Write(byte[] data)
        {
            lock (syncRoot)
            {
                jobs.Enqueue(data);
            }
        }

        protected virtual void OnRead(Byte[] bytes)
        {

        }

        protected virtual void OnClose()
        {

        }

        public void Dispose()
        {
            IsClosed = true;
            if (!disposed)
            {
                disposed = true;
                net?.Close();
                net = null;
                client?.Close();
                client = null;

                // .NET では Thread.Abort が使えない。読み込みのスレッドは接続を閉じたことで、
                // 書き込みのスレッドは disposed を見て終わる
                readThread = null;
                writeThread = null;
            }
        }
    }
}
