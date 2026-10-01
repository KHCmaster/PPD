using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PPDMultiCommon.Tcp
{
    public delegate void ReadClientEventHandler(byte[] data, int id);
    public delegate void ClosedEventHandler(int id);
    public class Host : IDisposable
    {
        public event ReadClientEventHandler ReadClient;
        public event ClosedEventHandler Closed;
        private readonly ConnectionManager manager;
        private TcpListener _listener;

        public int ConnectionCount
        {
            get
            {
                return manager.ConnectionCount;
            }
        }

        public ConnectionBase[] Connections
        {
            get
            {
                return manager.Connections;
            }
        }

        public Host()
        {
            manager = new ConnectionManager();
        }

        public int Port
        {
            get;
            set;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            Console.WriteLine("Port{0}のListenを開始しました。", Port);

            _listener.BeginAcceptTcpClient(DoAcceptTcpClient, _listener);
        }

        public void Stop()
        {
            _listener?.Dispose();

            manager?.Close();
        }

        private void DoAcceptTcpClient(IAsyncResult result)
        {
            try
            {
                var listener = (TcpListener)result.AsyncState;
                var client = listener.EndAcceptTcpClient(result);
                client.SendBufferSize = 1024 * 1024 * 10;
                client.ReceiveBufferSize = 1024 * 1024 * 2;
                Console.WriteLine("クライアントが接続しました。");
                //NetworkStreamを取得
                var ns = client.GetStream();

                var connection = new Connection(client, ns);
                connection.ReadClient += Connection_ReadClient;
                connection.Closed += Connection_Closed;
                manager.Add(connection);
                connection.Start();

                listener.BeginAcceptTcpClient(DoAcceptTcpClient, listener);
            }
            catch
            {
            }
        }

        void Connection_Closed(int id)
        {
            Closed?.Invoke(id);
        }

        void Connection_ReadClient(byte[] data, int id)
        {
            ReadClient?.Invoke(data, id);
        }

        public void Write(byte[] data)
        {
            manager.Write(data);
        }

        public void Write(byte[] data, int id)
        {
            manager.Write(data, id);
        }

        public void WriteExceptID(byte[] data, int exceptID)
        {
            manager.WriteExceptID(data, exceptID);
        }

        public string GetIp(int id)
        {
            return manager.GetIp(id);
        }

        public void Close(ConnectionBase connection)
        {
            manager.Close(connection);
        }

        public ConnectionBase GetConnection(int id)
        {
            return manager.GetConnection(id);
        }

        class ConnectionManager
        {
            readonly SortedList<int, Connection> connections;

            public int ConnectionCount
            {
                get
                {
                    lock (connections)
                    {
                        return connections.Count;
                    }
                }
            }

            public Connection[] Connections
            {
                get
                {
                    lock (connections)
                    {
                        return [.. connections.Values];
                    }
                }
            }

            public ConnectionManager()
            {
                connections = [];
            }

            public void Add(Connection connection)
            {
                lock (connections)
                {
                    connections.Add(connection.ID, connection);
                }
                connection.Closed += Connection_Closed;
            }

            void Connection_Closed(int id)
            {
                lock (connections)
                {
                    try
                    {
                        Connection connection = connections[id];
                        connection.Dispose();
                        connections.Remove(id);
                    }
                    catch
                    {
                    }
                }
            }

            public void Write(byte[] data)
            {
                lock (connections)
                {
                    foreach (Connection connection in connections.Values)
                    {
                        connection.Write(data);
                    }
                }
            }

            public void Write(byte[] data, int id)
            {
                lock (connections)
                {
                    if (connections.TryGetValue(id, out Connection value))
                    {
                        value.Write(data);
                    }
                }
            }

            public void WriteExceptID(byte[] data, int exceptID)
            {
                lock (connections)
                {
                    foreach (Connection connection in connections.Values)
                    {
                        if (connection.ID == exceptID)
                        {
                            continue;
                        }
                        connection.Write(data);
                    }
                }
            }

            public string GetIp(int id)
            {
                lock (connections)
                {
                    if (connections.TryGetValue(id, out Connection value))
                    {
                        return value.Ip;
                    }
                }
                return null;
            }

            public ConnectionBase GetConnection(int id)
            {
                lock (connections)
                {
                    if (connections.TryGetValue(id, out Connection value))
                    {
                        return value;
                    }
                }
                return null;
            }

            public void Close()
            {
                lock (connections)
                {
                    for (int i = connections.Count - 1; i >= 0; i--)
                    {
                        Connection connection = connections.Values[i];
                        connection.Dispose();
                        connections.RemoveAt(i);
                    }
                }
            }

            public void Close(ConnectionBase connection)
            {
                lock (connections)
                {
                    try
                    {
                        if (connection is not Connection c)
                        {
                            return;
                        }
                        if (!connections.ContainsKey(c.ID))
                        {
                            return;
                        }
                        connection.Dispose();
                        connections.Remove(c.ID);
                    }
                    catch
                    {
                    }
                }
            }
        }


        class Connection(TcpClient client, NetworkStream net) : ConnectionBase(client, net)
        {
            public static int _id;
            public event ReadClientEventHandler ReadClient;
            public event ClosedEventHandler Closed;
            private readonly int id = ++_id;

            public int ID
            {
                get { return id; }
            }

            public string Ip
            {
                get
                {
                    return ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                }
            }

            protected override void OnRead(byte[] bytes)
            {
                if (bytes != null && ReadClient != null)
                {
                    ReadClient.Invoke(bytes, id);
                }
            }

            protected override void OnClose()
            {
                Closed?.Invoke(id);
            }
        }

        /// <summary>
        /// 待ち受けと接続を終了します（Stop() と同じ）。
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
                Stop();
            }
        }
    }
}
