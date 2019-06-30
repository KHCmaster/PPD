using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace PPDMultiCommon.Tcp
{
    public abstract class ConnectionBase
    {
        protected TcpClient client;
        NetworkStream net;
        Thread readThread;
        Thread writeThread;
        Queue<byte[]> jobs;

        bool disposed;

        public bool IsClosed
        {
            get;
            protected set;
        }

        public DateTime LastDataReceivedTime
        {
            get;
            protected set;
        }

        protected ConnectionBase(TcpClient client, NetworkStream net)
        {
            this.client = client;
            this.net = net;
            jobs = new Queue<byte[]>();
            LastDataReceivedTime = DateTime.Now;
        }

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
                using (MemoryStream ms = new MemoryStream())
                {
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
        }

        private void WriteThread()
        {
            while (true)
            {
                if (!IsClosed && jobs.Count > 0)
                {
                    try
                    {
                        if (net.CanWrite)
                        {
                            byte[] sendData;
                            lock (this)
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

            OnClose();
        }

        public void Write(byte[] data)
        {
            lock (this)
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
                if (net != null)
                {
                    net.Close();
                    net = null;
                }
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                if (readThread != null)
                {
                    readThread.Abort();
                    readThread = null;
                }

                if (writeThread != null)
                {
                    writeThread.Abort();
                    writeThread = null;
                }
            }
        }
    }
}
