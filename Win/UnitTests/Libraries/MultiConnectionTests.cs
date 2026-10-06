using MessagePack;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// マルチプレイの通信（部屋を作る側の Host と、入る側の Client）のテストです。
    /// </summary>
    /// <remarks>
    /// Host は localhost（IPAddress.Loopback）の空いているポートだけで待ち受けるので、ファイアウォールの確認は出ない。
    /// </remarks>
    public class MultiConnectionTests
    {
        static byte[] Serialize(NetworkData data) => MessagePackSerializer.Serialize(data.GetType(), data);

        /// <summary>
        /// Host に入るクライアントです。受け取った値を、届いた順に待てます。
        /// </summary>
        sealed class Player
        {
            readonly TcpByteReader reader = new();
            public Client Client { get; }
            public TestEvents Events { get; } = new();
            public ConcurrentQueue<NetworkData> Received { get; } = new();

            public Player(int port)
            {
                Client = new Client { Address = "127.0.0.1", Port = port };
                reader.ByteReaded += (info, id) =>
                {
                    Received.Enqueue(info.NetworkData);
                    Events.Add("received " + info.NetworkData.Id);
                };
                Client.Read += data => reader.Read(data, 0);
                Client.Closed += () => Events.Add("closed");
                Client.Start();
            }
        }

        [Fact]
        public void HostAndClients_ExchangeMessages()
        {
            var port = TestEvents.FreePort();
            var hostEvents = new TestEvents();
            var hostReader = new TcpByteReader();
            var fromClients = new ConcurrentQueue<(NetworkData Data, int Id)>();
            hostReader.ByteReaded += (info, id) =>
            {
                fromClients.Enqueue((info.NetworkData, id));
                hostEvents.Add("received " + info.NetworkData.Id);
            };
            using var host = new Host { Port = port, ListenAddress = IPAddress.Loopback };
            host.ReadClient += hostReader.Read;
            host.Closed += id => hostEvents.Add("closed");
            host.Start();
            Assert.Equal(IPAddress.Any, new Host().ListenAddress);

            var first = new Player(port);
            var second = new Player(port);
            Assert.True(first.Client.HasConnection && second.Client.HasConnection);
            Assert.True(WaitUntil(() => host.ConnectionCount == 2));

            // クライアントから送る（続けて送っても 1 つずつ受け取る）
            first.Client.Write(Serialize(new AddMessageNetworkData { Message = "こんにちは", Id = 1 }));
            first.Client.Write(Serialize(new ChangeScoreNetworkData { Score = 100, Id = 2 }));
            hostEvents.WaitFor("received 1");
            hostEvents.WaitFor("received 2");
            Assert.True(fromClients.TryDequeue(out var message));
            Assert.Equal("こんにちは", ((AddMessageNetworkData)message.Data).Message);
            var firstId = message.Id;
            Assert.Equal("127.0.0.1", host.GetIp(firstId));
            Assert.NotNull(host.GetConnection(firstId));
            Assert.Null(host.GetConnection(-1));
            Assert.Null(host.GetIp(-1));

            // 全員に、1 人に、1 人以外に
            host.Write(Serialize(new ChangeLifeNetworkData { Life = 10, Id = 10 }));
            first.Events.WaitFor("received 10");
            second.Events.WaitFor("received 10");
            host.Write(Serialize(new ChangeLifeNetworkData { Life = 20, Id = 20 }), firstId);
            host.WriteExceptID(Serialize(new ChangeLifeNetworkData { Life = 30, Id = 30 }), firstId);
            first.Events.WaitFor("received 20");
            second.Events.WaitFor("received 30");
            host.Write(Serialize(new ChangeLifeNetworkData { Life = 40, Id = 40 }), -1);
            host.Write(Serialize(new ChangeLifeNetworkData { Life = 50, Id = 50 }));
            first.Events.WaitFor("received 50");
            second.Events.WaitFor("received 50");
            Assert.Equal([10, 20, 50], first.Received.Select(d => d.Id));
            Assert.Equal([10, 30, 50], second.Received.Select(d => d.Id));

            // クライアントが抜けると、Host から消える
            second.Client.Close();
            Assert.False(second.Client.HasConnection);
            Assert.True(WaitUntil(() => host.ConnectionCount == 1));

            // Host が切ると、クライアントは閉じる
            host.Close(host.GetConnection(firstId));
            Assert.Equal(0, host.ConnectionCount);
            first.Events.WaitFor("closed");
            Assert.False(first.Client.HasConnection);

            host.Stop();
            Assert.False(PPDExpansionCore.Tcp.Base.IsListening(port));
        }

        [Fact]
        public void Client_WithoutConnection_DoesNothing()
        {
            var client = new Client { Address = "127.0.0.1", Port = TestEvents.FreePort() };

            client.Write([1]);
            client.Close();

            Assert.False(client.HasConnection);
        }

        static bool WaitUntil(System.Func<bool> condition)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.Elapsed < TestEvents.Timeout)
            {
                if (condition())
                {
                    return true;
                }
                System.Threading.Thread.Sleep(10);
            }
            return false;
        }
    }
}
