using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PPDExpansionCore.Tcp
{
    public class Host : Base
    {
        private TcpListener listener;
        private TcpClient client;
        private Thread thread;
        private bool closed;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<PackableBase> DataReceived;

        public int Port
        {
            get;
            private set;
        }

        public Host(int port)
        {
            Port = port;
        }

        public void Start()
        {
            listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();
            WaitConnected();
        }

        private void WaitConnected()
        {
            IAsyncResult result = null;
            result = listener.BeginAcceptTcpClient(state =>
            {
                if (listener == null)
                {
                    return;
                }
                client = listener.EndAcceptTcpClient(result);
                thread = new Thread(Work);
                thread.Start();
                OnConnected();
            }, listener);
        }

        public void Close()
        {
            closed = true;
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

        private void Work()
        {
            byte[] lastBytes = new byte[0];
            while (true)
            {
                var bytes = Read(client.GetStream());
                if (bytes.Length == 0)
                {
                    break;
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
            OnDisconnected();
            if (!closed)
            {
                WaitConnected();
            }
        }

        private byte[] WorkImpl(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                using (PPDPack.V2.PackReader reader = new PPDPack.V2.PackReader(stream))
                {
                    var dataType = (DataType)ParseInt(reader, "DataType");
                    var packable = PackableBase.Create(dataType);
                    if (packable != null)
                    {
                        packable.Read(reader);
                        OnDataReceived(packable);
                    }

                    byte[] restBytes = new byte[bytes.Length - reader.FileSize];
                    if (restBytes.Length > 0)
                    {
                        Array.Copy(bytes, reader.FileSize, restBytes, 0, restBytes.Length);
                    }
                    return restBytes;
                }
            }
            catch
            {
                return bytes;
            }
        }

        private void OnDataReceived(PackableBase packable)
        {
            DataReceived?.Invoke(packable);
        }

        private void OnConnected()
        {
            Connected?.Invoke();
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke();
        }
    }
}
