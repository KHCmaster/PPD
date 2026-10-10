using PPDExpansionCore;
using PPDExpansionCore.Tcp;
using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ゲームから拡張機能（PPDExpansion）へ値を送る通信（Client と Host）のテストです。
    /// </summary>
    /// <remarks>
    /// 待ち受けは localhost（IPAddress.Loopback）の空いているポートだけなので、ファイアウォールの確認は出ない。
    /// </remarks>
    public class ExpansionConnectionTests
    {
        [Fact]
        public void ClientSendsToHost_AndHostWaitsForTheNextClient()
        {
            var port = TestEvents.FreePort();
            var events = new TestEvents();
            var received = new ConcurrentQueue<PackableBase>();
            using var host = new Host(port);
            host.Connected += () => events.Add("connected");
            host.Disconnected += () => events.Add("disconnected");
            host.DataReceived += data =>
            {
                received.Enqueue(data);
                events.Add("received " + data.DataType);
            };
            host.Start();
            Assert.True(Base.IsListening(port));
            Assert.Equal(port, host.Port);

            using (var client = new Client(port))
            {
                var stopwatch = Stopwatch.StartNew();
                client.Start();
                // localhost の IPv4 にすぐつながる（IPv6 で断られるのを待たない）
                Assert.True(stopwatch.Elapsed.TotalSeconds < 1, stopwatch.Elapsed.ToString());
                Assert.True(client.Connected);
                Assert.Equal(port, client.Port);
                events.WaitFor("connected");

                // 続けて送っても、1 つずつ受け取る
                client.Send(new ScoreInfo { ScoreName = "曲", Difficulty = PPDFrameworkCore.Difficulty.Hard, ScoreHash = [1, 2, 3], StartTime = 1.5f });
                // 文字列は null にしない（ゲストのアカウントにも ID がある）
                client.Send(new PlayerInfo { UserName = "player", AcccountId = "Guest1", PlayerId = 7, IsSelf = true });
                events.WaitFor("received ScoreInfo");
                events.WaitFor("received PlayerInfo");
            }
            // 切れたら、次のゲームを待つ
            events.WaitFor("disconnected");

            Assert.True(received.TryDequeue(out var first));
            var score = Assert.IsType<ScoreInfo>(first);
            Assert.Equal(("曲", PPDFrameworkCore.Difficulty.Hard, 1.5f), (score.ScoreName, score.Difficulty, score.StartTime));
            Assert.Equal([1, 2, 3], score.ScoreHash);
            Assert.True(received.TryDequeue(out var second));
            var player = Assert.IsType<PlayerInfo>(second);
            Assert.Equal(("player", "Guest1", 7, true), (player.UserName, player.AcccountId, player.PlayerId, player.IsSelf));

            using (var next = new Client(port))
            {
                next.Start();
                events.WaitFor("connected");
                next.Send(new UpdateInfo());
                events.WaitFor("received UpdateInfo");
            }

            host.Close();
            Assert.False(Base.IsListening(port));
        }

        [Fact]
        public void ClosedClient_DoesNotSend()
        {
            var client = new Client(TestEvents.FreePort());

            // つながっていないときは何もしない
            client.Send(new UpdateInfo());
            client.Close();

            Assert.False(client.Connected);
        }
    }
}
