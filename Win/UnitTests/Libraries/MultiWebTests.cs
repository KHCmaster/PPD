using PPDMultiCommon.Web;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// マルチプレイの部屋の一覧のサーバーとのやりとり（PPDMultiCommon の WebManager と、それを別のスレッドで動かす Executor）のテストです。
    /// 接続先は localhost のテスト用のサーバーにする（本物のサーバーには接続しない）。
    /// </summary>
    public sealed class MultiWebTests : IDisposable
    {
        static readonly FieldInfo baseUrlField = typeof(WebManager).GetField("baseUrl", BindingFlags.Static | BindingFlags.NonPublic);
        readonly object savedBaseUrl = baseUrlField.GetValue(null);
        readonly WebTestScope scope = new();

        TestWebServer Server => scope.Server;

        public MultiWebTests()
        {
            baseUrlField.SetValue(null, Server.BaseUrl + "/multi-api");
        }

        public void Dispose()
        {
            baseUrlField.SetValue(null, savedBaseUrl);
            scope.Dispose();
        }

        static T Run<T>(T executor) where T : ExecutorBase
        {
            using var finished = new System.Threading.ManualResetEventSlim();
            executor.Finished += (sender, e) => finished.Set();
            executor.Start();
            Assert.True(finished.Wait(TestEvents.Timeout));
            Assert.True(executor.IsFinished);
            return executor;
        }

        [Fact]
        public void Room_CreateUpdateAndDelete()
        {
            Server.OnXml("/multi-api/createroom", """<Root Token="room-token" />""");
            Server.On("/multi-api/updateroominfo", request => TestWebServer.Status(request.Query["playercount"] == "9" ? HttpStatusCode.BadRequest : HttpStatusCode.OK));
            Server.On("/multi-api/deleteroom", _ => TestWebServer.Status(HttpStatusCode.OK));
            var web = new WebManager();
            var room = new RoomInfo(" ", "部屋 1", "secret", 12345);

            // 部屋がないうちは、更新も削除も送らない
            Assert.True(Run(new UpdateRoomInfoExecutor(web, 2, "hash")).Success);
            Assert.True(Run(new DeleteRoomExecutor(web)).Success);
            Assert.Empty(Server.Requests);

            Assert.True(Run(new CreateRoomExecutor(web, room)).Success);
            var create = Server.Requests.Single();
            Assert.Equal(("NONAME", "部屋 1", room.PasswordHash, "12345"), (create.Query["username"], create.Query["roomname"], create.Query["passwordhash"], create.Query["port"]));

            Assert.True(Run(new UpdateRoomInfoExecutor(web, 2, "hash")).Success);
            Assert.Equal(("room-token", "2", "hash"), (Server.Requests.Last().Query["token"], Server.Requests.Last().Query["playercount"], Server.Requests.Last().Query["scorehash"]));
            // 400 なら失敗
            Assert.False(Run(new UpdateRoomInfoExecutor(web, 9, "hash")).Success);

            Assert.True(Run(new DeleteRoomExecutor(web)).Success);
            Assert.Equal("room-token", Server.Requests.Last().Query["token"]);
            // 消した後は送らない
            var count = Server.Requests.Count;
            Run(new DeleteRoomExecutor(web));
            Assert.Equal(count, Server.Requests.Count);
        }

        [Fact]
        public void Room_NamesWithSymbolsAreSentAsTheyAre()
        {
            Server.OnXml("/multi-api/createroom", """<Root Token="t" />""");

            Assert.True(Run(new CreateRoomExecutor(new WebManager(), new RoomInfo("A&B #1", "R&D", "", 1))).Success);

            var create = Server.Requests.Single();
            Assert.Equal(("A&B #1", "R&D", "1"), (create.Query["username"], create.Query["roomname"], create.Query["port"]));
        }

        [Fact]
        public void Room_CreateFailsWhenServerFails()
        {
            Server.On("/multi-api/createroom", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));

            Assert.False(Run(new CreateRoomExecutor(new WebManager(), new RoomInfo("user", "room", "", 1))).Success);
        }

        [Fact]
        public void RoomList_AndScores()
        {
            Server.OnXml("/multi-api/getroomlist", """
                <Root><Room UserName="u" RoomName="r" PasswordHash="" Port="100" Language="jpn" IP="10.0.0.1" PlayerCount="3" /><Room UserName="v" RoomName="s" PasswordHash="AB" Port="200" Language="eng" IP="10.0.0.2" PlayerCount="1" /></Root>
                """);
            Server.OnXml("/api/all-score-list", """<Root><Score Title="曲" Easy="0102" Normal="" Hard="" Extreme="ff" Hash="ABC" MovieUrl="m" /></Root>""");

            var rooms = Run(new GetRoomListExecutor()).RoomList;

            Assert.Equal([("u", "r", 100, "jpn", "10.0.0.1", 3, false), ("v", "s", 200, "eng", "10.0.0.2", 1, true)],
                rooms.Select(r => (r.UserName, r.RoomName, r.Port, r.Language, r.IP, r.PlayerCount, r.HasPassword)));

            var score = WebManager.GetScores().Single();
            Assert.Equal(("曲", "ABC", "m"), (score.Title, score.Hash, score.MovieUrl));
            // ハッシュは 32 バイト（足りない分は 0）。ないハッシュは空
            Assert.Equal(32, score.Easy.Length);
            Assert.Equal([1, 2], score.Easy[..2]);
            Assert.Empty(score.Normal);
            Assert.Equal(0xff, score.Extreme[0]);
            Assert.Equal(Server.BaseUrl + "/score/index/id/abc", score.CreateUrl());

            Server.On("/multi-api/getroomlist", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Server.On("/api/all-score-list", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Empty(Run(new GetRoomListExecutor()).RoomList);
            Assert.Empty(WebManager.GetScores());
        }

        [Fact]
        public void PasswordHash_IsUpperHexSha256()
        {
            var hash = WebManager.GetPasswordHash("secret");

            Assert.Equal(64, hash.Length);
            Assert.Equal(hash.ToUpperInvariant(), hash);
            Assert.Equal(hash, new RoomInfo("u", "r", "secret", 1).PasswordHash);
            Assert.NotEqual(hash, WebManager.GetPasswordHash("other"));
            // パスワードがなければ空
            Assert.False(new RoomInfo("u", "r", "", 1).HasPassword);
            Assert.Equal("NOROOMNAME", new RoomInfo("u", " ", "", 1).RoomName);
        }
    }
}
