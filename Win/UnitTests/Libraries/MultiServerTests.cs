using MessagePack;
using PPDFrameworkCore;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using PPDMultiServer.Tcp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// マルチプレイのサーバー（PPDServer）のテストです。部屋に入る、話す、リーダー、キック、曲を選んで遊んで結果を見る、を通信で試します。
    /// </summary>
    /// <remarks>
    /// サーバーは localhost（127.0.0.1）の空いているポートだけで待ち受けるので、ファイアウォールの確認は出ない。
    /// 参加者は 127.0.0.x の別のアドレスからつなぐ（違うコンピューターの代わり）。
    /// 部屋の一覧のサーバー（WebManager）は使わない。
    /// </remarks>
    public sealed class MultiServerTests : IDisposable
    {
        static readonly string version = FileVersionInfo.GetVersionInfo(typeof(PPDServer).Assembly.Location).FileVersion;

        /// <summary>
        /// タイマーを動かさず、登録された処理をテストから呼べるようにします。
        /// </summary>
        sealed class ManualTimer : ITimerManager
        {
            public Dictionary<int, Action<int>> Callbacks { get; } = [];
            int next;

            public int AddTimerCallBack(Action<int> callback, int milliSec, bool onceExecute, bool immediate)
            {
                Callbacks[++next] = callback;
                return next;
            }

            public void RemoveTimerCallBack(int id)
            {
                Callbacks.Remove(id);
            }
        }

        /// <summary>
        /// 部屋に入る人です。サーバーから届いた値を順に受け取ります。
        /// </summary>
        sealed class Player : IDisposable
        {
            readonly MultiServerTests test;
            readonly TcpClient tcp;
            readonly NetworkStream stream;
            readonly TcpByteReader reader = new();
            readonly BlockingCollection<NetworkData> received = [];
            readonly List<string> skipped = [];

            public int Id { get; set; }

            public Player(MultiServerTests test, string address)
            {
                this.test = test;
                tcp = new TcpClient(new IPEndPoint(IPAddress.Parse(address), 0));
                tcp.Connect(IPAddress.Loopback, test.port);
                stream = tcp.GetStream();
                reader.ByteReaded += (info, id) => received.Add(info.NetworkData);
                new Thread(() =>
                {
                    var buffer = new byte[65536];
                    try
                    {
                        int size;
                        while ((size = stream.Read(buffer)) > 0)
                        {
                            reader.Read(buffer[..size], 0);
                        }
                    }
                    catch (Exception ex) when (ex is IOException or ObjectDisposedException)
                    {
                    }
                    received.Add(new CloseNetworkData());
                })
                { IsBackground = true }.Start();
            }

            public void Send(NetworkData data)
            {
                stream.Write(MessagePackSerializer.Serialize(data.GetType(), data));
            }

            public void SendRaw(byte[] bytes)
            {
                stream.Write(bytes);
            }

            /// <summary>
            /// 条件に合う値が届くまで、サーバーを動かしながら待ちます。それまでに届いたほかの値は読み捨てます。
            /// </summary>
            public T Expect<T>(Func<T, bool> match = null) where T : NetworkData
            {
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.Elapsed < TestEvents.Timeout)
                {
                    test.server.Update();
                    if (received.TryTake(out var data, 10))
                    {
                        if (data is T t && (match == null || match(t)))
                        {
                            return t;
                        }
                        skipped.Add(data.GetType().Name);
                    }
                }
                throw new TimeoutException(typeof(T).Name + " が届きません。届いたもの: " + string.Join(", ", skipped));
            }

            public void Dispose()
            {
                tcp.Dispose();
            }
        }

        readonly int port = TestEvents.FreePort();
        readonly ManualTimer timer = new();
        readonly TempFolder logs = new();
        readonly PPDServer server;
        readonly List<Player> players = [];

        public MultiServerTests()
        {
            server = new PPDServer(port, null, null, timer, logs.Path, ["mod1"]) { ListenAddress = IPAddress.Loopback };
            server.Start();
        }

        public void Dispose()
        {
            foreach (var player in players)
            {
                player.Dispose();
            }
            server.Dispose();
            logs.Dispose();
        }

        Player Connect(string address)
        {
            var player = new Player(this, address);
            players.Add(player);
            return player;
        }

        /// <summary>
        /// 部屋に入ります。入れたときはサーバーの情報、断られたときは閉じる理由が最初に届きます。
        /// </summary>
        static NetworkData Join(Player player, string name, string playerVersion = null)
        {
            player.Send(new AddUserNetworkData { UserName = name, AccountId = "@" + name, Version = playerVersion ?? version, State = UserState.NotReady });
            var first = player.Expect<NetworkData>(d => d is SendServerInfoNetworkData or CloseConnectNetworkData);
            if (first is SendServerInfoNetworkData info)
            {
                player.Id = info.Id;
                Assert.Equal(["mod1"], info.AllowedModIds);
            }
            return first;
        }

        (Player Leader, Player Member) TwoPlayers()
        {
            var leader = Connect("127.0.0.1");
            Assert.IsType<SendServerInfoNetworkData>(Join(leader, "A"));
            leader.Expect<ChangeLeaderNetworkData>(d => d.UserId == leader.Id);
            var member = Connect("127.0.0.2");
            Assert.IsType<SendServerInfoNetworkData>(Join(member, "B"));
            // 後から入った人には、前からいる人とリーダーを知らせる
            Assert.Equal("A", member.Expect<AddUserNetworkData>(d => d.Id == leader.Id).UserName);
            member.Expect<ChangeLeaderNetworkData>(d => d.UserId == leader.Id);
            // 前からいる人には、入った人を知らせる
            Assert.Equal("@B", leader.Expect<AddUserNetworkData>(d => d.Id == member.Id).AccountId);
            return (leader, member);
        }

        [Fact]
        public void Menu_ChatStatePingAndLeader()
        {
            var (a, b) = TwoPlayers();
            Assert.Equal(2, server.ConnectionCount);
            Assert.Equal(IPAddress.Loopback, server.ListenAddress);

            // 版の違う人は入れない
            var old = Connect("127.0.0.3");
            Assert.Equal(CloseConnectReason.VersionDifference, Assert.IsType<CloseConnectNetworkData>(Join(old, "C", "0.0.0.0")).Reason);

            a.Send(new AddMessageNetworkData { Message = "こんにちは" });
            Assert.Equal(a.Id, b.Expect<AddMessageNetworkData>(d => d.Message == "こんにちは").Id);
            a.Send(new AddPrivateMessageNetworkData { Message = "ひみつ", UserId = b.Id });
            Assert.Equal(a.Id, b.Expect<AddPrivateMessageNetworkData>(d => d.Message == "ひみつ").Id);
            b.Send(new ChangeUserStateNetworkData { State = UserState.Watch });
            a.Expect<ChangeUserStateNetworkData>(d => d.Id == b.Id && d.State == UserState.Watch);

            // 応答の速さを全員に知らせる
            a.Send(new PingNetworkData { Time = 0 });
            b.Expect<PingUserNetworkData>(d => d.Id == a.Id);
            // 定期的に、全員に応答の速さを測る値を送る
            Assert.Single(timer.Callbacks).Value(0);
            b.Expect<PingNetworkData>();

            // リーダーだけがリーダーを譲れる
            b.Send(new ChangeLeaderNetworkData { UserId = b.Id });
            a.Send(new ChangeLeaderNetworkData { UserId = b.Id });
            b.Expect<ChangeLeaderNetworkData>(d => d.UserId == b.Id);

            // リーダーが抜けると、残った人がリーダー（新しいリーダーを先に知らせる）
            b.Dispose();
            a.Expect<ChangeLeaderNetworkData>(d => d.UserId == a.Id);
            a.Expect<DeleteUserNetworkData>(d => d.Id == b.Id);
        }

        [Fact]
        public void FullRoom_RefusesTheSeventeenth()
        {
            for (var i = 1; i <= 16; i++)
            {
                Assert.IsType<SendServerInfoNetworkData>(Join(Connect("127.0.0." + i), "P" + i));
            }

            var late = Connect("127.0.0.17");

            Assert.Equal(CloseConnectReason.MaximumCapacity, Assert.IsType<CloseConnectNetworkData>(Join(late, "late")).Reason);
        }

        [Fact]
        public void ForceStart_StartsWithoutPlayersWhoLackTheSong()
        {
            var (a, b) = TwoPlayers();
            a.Send(new ChangeSongNetworkData { Hash = [1], Difficulty = Difficulty.Easy });
            b.Expect<ChangeSongNetworkData>();
            a.Send(new HasSongNetworkData());
            b.Expect<HasSongNetworkData>(d => d.Id == a.Id);
            a.Send(new ChangeUserStateNetworkData { State = UserState.Ready });
            b.Expect<ChangeUserStateNetworkData>(d => d.Id == a.Id);

            // 曲のない人がいると始まらない。リーダーが強制すると、曲のある人だけで始める
            b.Send(new ForceStartNetworkData());
            a.Send(new ForceStartNetworkData());
            b.Expect<ForceStartNetworkData>();
            a.Expect<GoToPlayPrepareNetworkData>();
            Assert.Equal([a.Id], a.Expect<GoToPlayNetworkData>().PlayerIds);

            // 遊んでいる人が抜けたら、メニューに戻る
            a.Send(new MainGameLoadedNetworkData());
            a.Expect<PlayMainGameNetworkData>();
            a.Send(new JustGoToMenuNetworkData());
            a.Expect<JustGoToMenuNetworkData>();
            a.Send(new ChangeSongNetworkData { Hash = [2], Difficulty = Difficulty.Easy });
            Assert.Equal([2], b.Expect<ChangeSongNetworkData>(d => d.Hash[0] == 2).Hash);
        }

        [Fact]
        public void OldClient_IsDisconnected()
        {
            var old = Connect("127.0.0.1");

            // 以前の形式（PPDPACK で始まる）で送ってくるクライアントには、以前の形式で切断を伝えて切る
            old.SendRaw([.. "PPDPACKV2"u8, 0, 0, 0, 0]);

            old.Expect<CloseNetworkData>();
        }

        [Fact]
        public void RoomList_IsCreatedUpdatedAndDeleted()
        {
            var baseUrlField = typeof(PPDMultiCommon.Web.WebManager).GetField("baseUrl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var savedBaseUrl = baseUrlField.GetValue(null);
            using var scope = new WebTestScope();
            baseUrlField.SetValue(null, scope.Server.BaseUrl + "/multi-api");
            try
            {
                scope.Server.OnXml("/multi-api/createroom", """<Root Token="t1" />""");
                scope.Server.On("/multi-api/updateroominfo", _ => TestWebServer.Status(HttpStatusCode.BadRequest));
                scope.Server.On("/multi-api/deleteroom", _ => TestWebServer.Status(HttpStatusCode.OK));
                var webTimer = new ManualTimer();
                var room = new PPDMultiCommon.Web.RoomInfo("host", "room", "", 1);
                using (var webServer = new PPDServer(TestEvents.FreePort(), new PPDMultiCommon.Web.WebManager(), room, webTimer) { ListenAddress = IPAddress.Loopback })
                {
                    webServer.Start();
                    // 部屋を作ったら、定期的に人数を知らせる
                    Assert.True(SpinWait.SpinUntil(() => webTimer.Callbacks.Count == 2, TestEvents.Timeout));
                    Assert.Equal("room", scope.Server.Requests.First().Query["roomname"]);
                    webTimer.Callbacks.Values.ToList().ForEach(callback => callback(0));
                    // 知らせるのに失敗したら、部屋を作り直す
                    Assert.True(SpinWait.SpinUntil(() => scope.Server.Requests.Count(r => r.Path == "/multi-api/createroom") == 2, TestEvents.Timeout));
                    Assert.Equal("0", scope.Server.Requests.Single(r => r.Path == "/multi-api/updateroominfo").Query["playercount"]);
                }
                // 閉じたら部屋を消す
                Assert.True(SpinWait.SpinUntil(() => scope.Server.Requests.Any(r => r.Path == "/multi-api/deleteroom"), TestEvents.Timeout));

                // 部屋を作れなければ知らせる
                scope.Server.On("/multi-api/createroom", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
                using var failed = new PPDServer(TestEvents.FreePort(), new PPDMultiCommon.Web.WebManager(), room, new ManualTimer()) { ListenAddress = IPAddress.Loopback };
                var failedCount = 0;
                failed.FailedToCreateRoom += (sender, e) => failedCount++;
                failed.Start();
                Assert.True(SpinWait.SpinUntil(() =>
                {
                    failed.Update();
                    return failedCount == 1;
                }, TestEvents.Timeout));
            }
            finally
            {
                baseUrlField.SetValue(null, savedBaseUrl);
            }
        }

        [Fact]
        public void Kick_BlocksOnlyTheKickedAddress()
        {
            var (a, b) = TwoPlayers();

            // リーダーでない人はキックできない
            b.Send(new KickUserNetworkData { UserId = a.Id });
            a.Send(new KickUserNetworkData { UserId = b.Id });
            Assert.Equal(CloseConnectReason.Kicked, b.Expect<CloseConnectNetworkData>().Reason);
            a.Expect<KickUserNetworkData>(d => d.UserId == b.Id);

            // ほかのアドレスの人は入れる
            var c = Connect("127.0.0.3");
            Assert.IsType<SendServerInfoNetworkData>(Join(c, "C"));
            // キックした人は、つなぎ直しても入れない
            var again = Connect("127.0.0.2");
            Assert.Equal(CloseConnectReason.Kicked, Assert.IsType<CloseConnectNetworkData>(Join(again, "B")).Reason);
        }

        [Fact]
        public void Game_PrepareAndPlayWithResults()
        {
            var (a, b) = TwoPlayers();
            byte[] hash = [1, 2, 3];

            // リーダーが曲を選ぶ（リーダーでない人は選べない）
            b.Send(new ChangeSongNetworkData { Hash = [9], Difficulty = Difficulty.Easy });
            a.Send(new ChangeSongNetworkData { Hash = hash, Difficulty = Difficulty.Hard });
            Assert.Equal(hash, b.Expect<ChangeSongNetworkData>().Hash);
            a.Send(new ChangeGameRuleNetworkData { GameRule = new GameRule() });
            b.Expect<ChangeGameRuleNetworkData>();

            // 全員が持っている曲だけを、選べる曲にする
            var common = new SongInfo { Hash = hash, Difficulty = Difficulty.Hard };
            a.Send(new SendScoreListNetworkData { SongInfos = [common, new SongInfo { Hash = [7], Difficulty = Difficulty.Easy }] });
            b.Send(new SendScoreListNetworkData { SongInfos = [new SongInfo { Hash = hash, Difficulty = Difficulty.Hard }] });
            Assert.Equal(hash, Assert.Single(a.Expect<SendScoreListNetworkData>(d => d.SongInfos.Length == 1).SongInfos).Hash);

            // 曲を持っている全員が準備できたら、準備の画面へ。5 秒後に遊ぶ
            a.Send(new HasSongNetworkData());
            b.Send(new HasSongNetworkData());
            a.Send(new ChangeUserStateNetworkData { State = UserState.Ready });
            b.Send(new ChangeUserStateNetworkData { State = UserState.Ready });
            a.Expect<GoToPlayPrepareNetworkData>();
            b.Expect<GoToPlayPrepareNetworkData>();
            Assert.Equal(new[] { a.Id, b.Id }.Order(), a.Expect<GoToPlayNetworkData>().PlayerIds.Order());
            b.Expect<GoToPlayNetworkData>();

            // 全員が読み込んだら始める
            a.Send(new MainGameLoadedNetworkData());
            b.Send(new MainGameLoadedNetworkData());
            b.Expect<PlayMainGameNetworkData>();

            // 遊んでいる間の値は、ほかの人に送る
            a.Send(new ChangeScoreNetworkData { Score = 1234 });
            a.Send(new ChangeLifeNetworkData { Life = 40 });
            a.Send(new ChangeEvaluateNetworkData { Evaluate = MarkEvaluateTypeEx.Cool });
            Assert.Equal(1234, b.Expect<ChangeScoreNetworkData>(d => d.Id == a.Id).Score);
            Assert.Equal(40, b.Expect<ChangeLifeNetworkData>(d => d.Id == a.Id).Life);
            b.Expect<ChangeEvaluateNetworkData>(d => d.Id == a.Id);
            a.Send(new AddEffectToPlayerNetworkData { UserId = b.Id, ItemType = ItemType.Auto });
            b.Expect<AddEffectToPlayerNetworkData>(d => d.Id == a.Id);
            a.Send(new AddEffectNetworkData { ItemType = ItemType.Auto });
            b.Expect<AddEffectToPlayerNetworkData>(d => d.Id == a.Id);

            // 全員の結果が揃ったら、結果を持ってメニューへ
            b.Send(new SendResultNetworkData { Result = new Result { Score = 500 } });
            a.Send(new SendResultNetworkData { Result = new Result { Score = 1234, CoolCount = 3 } });
            var results = a.Expect<GoToMenuNetworkData>().Results;
            Assert.Equal([(b.Id, 500), (a.Id, 1234)], results.Select(r => (r.Item1, r.Item2.Score)));

            // メニューに戻ったら、また曲を選べる
            a.Send(new ChangeSongNetworkData { Hash = [4], Difficulty = Difficulty.Normal });
            Assert.Equal([4], b.Expect<ChangeSongNetworkData>(d => d.Difficulty == Difficulty.Normal).Hash);

            server.Close();
            // 結果はログに残る
            var log = File.ReadAllText(Directory.GetFiles(logs.Path).Single());
            Assert.Contains("Main game started", log);
            Assert.Contains("Score:1234 C:3", log);
        }
    }
}
