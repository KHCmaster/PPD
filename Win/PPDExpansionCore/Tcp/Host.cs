using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PPDExpansionCore.Tcp
{
    public class Host : Base, IDisposable
    {
        private TcpListener listener;
        private TcpClient client;
        private Thread thread;
        private volatile bool closed;

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
                var currentListener = listener;
                if (currentListener == null)
                {
                    return;
                }
                try
                {
                    client = currentListener.EndAcceptTcpClient(result);
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException || ex is InvalidOperationException)
                {
                    // Close で待ち受けを止めた
                    return;
                }
                thread = new Thread(Work) { IsBackground = true };
                thread.Start();
                OnConnected();
            }, listener);
        }

        public void Close()
        {
            closed = true;
            // .NET では Thread.Abort が使えない。受信のスレッドは、接続を閉じると Read が終わるので止まる
            thread = null;
            client?.Close();
            client = null;
            listener?.Dispose();
            listener = null;
        }

        private void Work()
        {
            byte[] lastBytes = new byte[0];
            NetworkStream stream;
            try
            {
                stream = client.GetStream();
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException || ex is NullReferenceException)
            {
                return;
            }
            while (true)
            {
                var bytes = Read(stream);
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
            if (closed)
            {
                // Close で止められた（以前は Thread.Abort で止めていたので、OnDisconnected は呼ばない）
                return;
            }
            OnDisconnected();
            WaitConnected();
        }

        private byte[] WorkImpl(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new(bytes))
                using (PPDPack.V2.PackReader reader = new(stream))
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
