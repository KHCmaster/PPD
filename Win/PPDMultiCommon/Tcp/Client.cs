using System;
using System.Net.Sockets;

namespace PPDMultiCommon.Tcp
{
    public delegate void ReadEventHandler(byte[] data);
    public class Client
    {
        public event ReadEventHandler Read;
        public event Action Closed;
        private Connection connection;

        public string Address
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public void Start()
        {
            TcpClient tcp = null;
            try
            {
                tcp = new TcpClient(Address, Port)
                {
                    SendBufferSize = 1024 * 1024 * 2,
                    ReceiveBufferSize = 1024 * 1024 * 10
                };
            }
            catch
            {
                return;
            }
            Console.WriteLine("サーバーと接続しました。");

            var ns = tcp.GetStream();

            connection = new Connection(tcp, ns);
            connection.Read += connection_Read;
            connection.Closed += connection_Closed;
            connection.Start();
        }

        void connection_Closed()
        {
            if (Closed != null)
            {
                Closed.Invoke();
            }
        }

        void connection_Read(byte[] data)
        {
            if (Read != null)
            {
                Read.Invoke(data);
            }
        }

        public void Write(byte[] data)
        {
            if (connection != null)
            {
                connection.Write(data);
            }
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }

        public bool HasConnection
        {
            get
            {
                return connection != null && !connection.IsClosed;
            }
        }

        class Connection : ConnectionBase
        {
            public event ReadEventHandler Read;
            public event Action Closed;

            public Connection(TcpClient client, NetworkStream net)
                : base(client, net)
            {
            }

            protected override void OnRead(byte[] bytes)
            {
                if (bytes != null && Read != null)
                {
                    Read.Invoke(bytes);
                }
            }

            protected override void OnClose()
            {
                if (Closed != null)
                {
                    Closed.Invoke();
                }
            }
        }
    }
}
