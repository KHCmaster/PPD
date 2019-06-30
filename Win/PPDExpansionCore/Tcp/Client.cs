using System;
using System.IO;
using System.Net.Sockets;

namespace PPDExpansionCore.Tcp
{
    public class Client : Base
    {
        private TcpClient tcpClient;
        private bool connected;

        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        public int Port
        {
            get;
            private set;
        }

        public Client(int port)
        {
            Port = port;
        }

        public void Start()
        {
            try
            {
                tcpClient = new TcpClient("localhost", Port);
                connected = true;
            }
            catch
            {

            }
        }

        public void Close()
        {
            if (!connected)
            {
                return;
            }

            tcpClient.Close();
            connected = false;
        }

        public void Send(PackableBase packable)
        {
            if (!connected || !tcpClient.Connected)
            {
                return;
            }
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (PPDPack.V2.PackWriter writer = new PPDPack.V2.PackWriter(stream))
                    {
                        packable.Write(writer);
                    }
                    stream.Close();
                    Send(stream.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Send Error: {0}", e.Message);
                Console.WriteLine("Send Error: {0}", e.StackTrace);
            }
        }

        private void Send(byte[] bytes)
        {
            if (!tcpClient.Connected)
            {
                return;
            }
            tcpClient.GetStream().Write(bytes, 0, bytes.Length);
        }

        protected override void OnClosed()
        {
            connected = false;
            Close();
        }
    }
}
