using PPDFramework.Web;
using System.Linq;
using System.Net;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// PPD の Web（WebManager）のテストです。接続先は localhost のテスト用のサーバーにする（本物のサーバーには接続しない）。
    /// </summary>
    public class WebManagerTests
    {
        const string LoginXml = """<Root Result="0" AccountID="acc1" Nickname="tester" Token="tok" PPDY="12" />""";

        static void Login(WebTestScope scope)
        {
            scope.Server.OnXml("/api/login", LoginXml);
            Assert.Equal(ErrorReason.OK, scope.Web.Login("user", "pass"));
        }

        [Fact]
        public void Login_SetsAccountAndLogoutClearsIt()
        {
            using var scope = new WebTestScope();
            Assert.False(scope.Web.IsLogined);
            // ログインしていないときはゲスト
            Assert.StartsWith("Guest", scope.Web.CurrentAccountId);

            Login(scope);

            Assert.True(scope.Web.IsLogined);
            Assert.Equal(("acc1", "tester"), (scope.Web.CurrentAccountId, scope.Web.CurrentUserName));
            var request = scope.Server.Requests.Single();
            Assert.Equal(("GET", "user", "pass", "6"), (request.Method, request.Query["username"], request.Query["password"], request.Query["version"]));

            scope.Web.Logout();
            Assert.False(scope.Web.IsLogined);
        }

        [Fact]
        public void Login_ErrorResultOrServerError()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/login", """<Root Result="2" />""");
            Assert.Equal((ErrorReason)2, scope.Web.Login("user", "wrong"));
            Assert.False(scope.Web.IsLogined);

            scope.Server.On("/api/login", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Equal(ErrorReason.NetworkError, scope.Web.Login("user", "pass"));
            // 壊れた応答
            scope.Server.OnXml("/api/login", "<Root");
            Assert.Equal(ErrorReason.NetworkError, scope.Web.Login("user", "pass"));
        }

        [Fact]
        public void UpdateAccountInfo_OnlyWhenLoggedIn()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/update-account-info", """<Root Result="0" Nickname="renamed" PPDY="99" />""");

            // ログインしていなければ送らない
            scope.Web.UpdateAccountInfo();
            Assert.Empty(scope.Server.Requests);

            Login(scope);
            scope.Web.UpdateAccountInfo();

            Assert.Equal("renamed", scope.Web.CurrentUserName);
            var request = scope.Server.Requests.Last();
            Assert.Equal(("POST", "acc1"), (request.Method, request.Form["accountid"]));
            // トークンそのものは送らない
            Assert.Null(request.Form["token"]);

            // 失敗の結果なら変えない
            scope.Server.OnXml("/api/update-account-info", """<Root Result="3" Nickname="other" PPDY="1" />""");
            scope.Web.UpdateAccountInfo();
            Assert.Equal("renamed", scope.Web.CurrentUserName);
        }

        const string RankingXml = """
            <Root Result="0">
              <Ranks Difficulty="0"><Rank Nickname="a" Score="100" ID="1" Rank="1" /><Rank Nickname="b" Score="90" ID="2" Rank="2" /></Ranks>
              <Ranks Difficulty="1"><Rank Nickname="c" Score="80" ID="3" Rank="1" /></Ranks>
              <Ranks Difficulty="2" />
              <Ranks Difficulty="3"><Rank Nickname="d" Score="70" ID="4" Rank="1" /></Ranks>
            </Root>
            """;

        [Fact]
        public void Ranking_ReadsEachDifficulty()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/ranking", RankingXml);

            var ranking = scope.Web.GetRanking("score1");

            Assert.Equal("score1", scope.Server.Requests.Single().Query["id"]);
            Assert.Equal(["a", "b"], ranking.Easy.Select(r => r.Nickname));
            Assert.Equal((90, "2", 2), (ranking.Easy[1].Score, ranking.Easy[1].ID, ranking.Easy[1].Rank));
            Assert.Single(ranking.Normal);
            Assert.Empty(ranking.Hard);
            Assert.Equal("d", ranking.Extreme.Single().Nickname);

            scope.Server.OnXml("/api/ranking", """<Root Result="1" />""");
            Assert.Null(scope.Web.GetRanking("score1"));
            scope.Server.On("/api/ranking", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Null(scope.Web.GetRanking("score1"));
        }

        [Fact]
        public void RivalRanking_NeedsLogin()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/rival-ranking", RankingXml);

            // ログインしていなければ空（送らない）
            var empty = scope.Web.GetRivalRanking("score1");
            Assert.Empty(empty.Easy);
            Assert.Empty(scope.Server.Requests);

            Login(scope);
            var ranking = scope.Web.GetRivalRanking("score1");

            Assert.Equal(2, ranking.Easy.Length);
            var request = scope.Server.Requests.Last();
            Assert.Equal(("POST", "acc1", "score1"), (request.Method, request.Form["accountid"], request.Form["scoreid"]));

            scope.Server.OnXml("/api/rival-ranking", """<Root Result="1" />""");
            Assert.Null(scope.Web.GetRivalRanking("score1"));
            scope.Server.OnXml("/api/rival-ranking", "broken");
            Assert.Null(scope.Web.GetRivalRanking("score1"));
        }

        [Fact]
        public void ScoreDetail_FindsIdByHash()
        {
            using var scope = new WebTestScope();
            scope.Server.On("/api/find-score-id", request => request.Query["hash"] == "0a0b"
                ? TestWebServer.Xml("""<Root Result="0" ID="score9" StartTime="1.5" EndTime="20.25" />""")
                : TestWebServer.Xml("""<Root Result="1" />"""));

            var detail = scope.Web.GetScoreDetail([0x0a, 0x0b]);

            Assert.Equal(("score9", 1.5f, 20.25f), (detail.Id, detail.StartTime, detail.EndTime));
            Assert.Equal("score9", scope.Web.GetScoreId([0x0a, 0x0b]));
            // 見つからない譜面
            Assert.Null(scope.Web.GetScoreDetail("ffff"));
            Assert.Equal("", scope.Web.GetScoreId("ffff"));
        }
    }
}
