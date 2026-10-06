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
        public void PerfectTrialPrepare_ReturnsResultAndAttributes()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/perfect-trial-prepare", """<Root Result="0" Token="pt" Count="3" />""");

            // ログインしていなければ送らない
            Assert.Equal(ErrorReason.AuthFailed, scope.Web.PerfectTrialPrepare([1, 2], out var data));
            Assert.Empty(data);
            Assert.Empty(scope.Server.Requests);

            Login(scope);
            Assert.Equal(ErrorReason.OK, scope.Web.PerfectTrialPrepare([1, 2], out data));
            Assert.Equal(("pt", "3"), (data["Token"], data["Count"]));
            Assert.Equal("0102", scope.Server.Requests.Last().Form["scorehash"]);

            scope.Server.OnXml("/api/perfect-trial-prepare", """<Root Result="4" />""");
            Assert.Equal((ErrorReason)4, scope.Web.PerfectTrialPrepare("0102", out _));
            // 結果がない、数でない応答
            scope.Server.OnXml("/api/perfect-trial-prepare", "<Root />");
            Assert.Equal(ErrorReason.NetworkError, scope.Web.PerfectTrialPrepare("0102", out _));
            scope.Server.OnXml("/api/perfect-trial-prepare", """<Root Result="x" />""");
            Assert.Equal(ErrorReason.NetworkError, scope.Web.PerfectTrialPrepare("0102", out _));
            scope.Server.On("/api/perfect-trial-prepare", _ => TestWebServer.Status(HttpStatusCode.BadGateway));
            Assert.Equal(ErrorReason.NetworkError, scope.Web.PerfectTrialPrepare("0102", out _));
        }

        [Fact]
        public void Ghost_ReadsReplayData()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/ghost", """
                <Root Result="0"><Ghost ID="1" AccountID="a1" AccountName="A" ShowScore="1" ShowEvaluate="0" ShowLife="1">AQID</Ghost><Ghost ID="2" AccountID="a2" AccountName="B" ShowScore="0" ShowEvaluate="1" ShowLife="0">BAU=</Ghost></Root>
                """);
            Assert.Equal(ErrorReason.AuthFailed, scope.Web.GetGhost("hash", 5, out var ghosts));
            Assert.Empty(ghosts);

            Login(scope);
            Assert.Equal(ErrorReason.OK, scope.Web.GetGhost("hash", 5, out ghosts));

            Assert.Equal(("hash", "5"), (scope.Server.Requests.Last().Form["scorehash"], scope.Server.Requests.Last().Form["maxcount"]));
            Assert.Equal(2, ghosts.Length);
            Assert.Equal((1, "a1", "A", true, false, true), (ghosts[0].Id, ghosts[0].AccountId, ghosts[0].AccountName, ghosts[0].ShowScore, ghosts[0].ShowEvaluate, ghosts[0].ShowLife));
            Assert.Equal([1, 2, 3], ghosts[0].ReplayData);
            Assert.Equal([4, 5], ghosts[1].ReplayData);

            scope.Server.On("/api/ghost", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Equal(ErrorReason.NetworkError, scope.Web.GetGhost("hash", 5, out ghosts));
            Assert.Empty(ghosts);
        }

        [Fact]
        public void PlayResult_SendsCountsAndRetriesNetworkErrors()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/playresult2", """<Root Result="0" Rank="3" />""");
            Assert.Equal(ErrorReason.AuthFailed, scope.Web.PlayResult([1], 100, 1, 2, 3, 4, 5, 6, 1.5f, 2.5f, [7, 8], 0, null, out _));

            Login(scope);
            Assert.Equal(ErrorReason.OK, scope.Web.PlayResult([1], 100, 1, 2, 3, 4, 5, 6, 1.5f, 2.5f, [7, 8], 0, "trial", out var data));

            Assert.Equal("3", data["Rank"]);
            var form = scope.Server.Requests.Last().Form;
            Assert.Equal(["01", "100", "1", "2", "3", "4", "5", "6", "1.5", "2.5", "Bwg=", "trial"],
                new[] { "scorehash", "score", "coolcount", "goodcount", "safecount", "sadcount", "worstcount", "maxcombo", "starttime", "endtime", "inputs", "perfecttrialtoken" }.Select(k => form[k]));

            // 通信のエラーなら、決めた回数だけ送り直す
            scope.Server.On("/api/playresult2", _ => TestWebServer.Status(HttpStatusCode.ServiceUnavailable));
            var before = scope.Server.Requests.Count;
            Assert.Equal(ErrorReason.NetworkError, scope.Web.PlayResult([1], 100, 1, 2, 3, 4, 5, 6, 1.5f, 2.5f, [7, 8], 2, null, out _));
            Assert.Equal(3, scope.Server.Requests.Count - before);
            // 結果の失敗は送り直さない
            scope.Server.OnXml("/api/playresult2", """<Root Result="2" />""");
            before = scope.Server.Requests.Count;
            Assert.Equal((ErrorReason)2, scope.Web.PlayResult([1], 100, 1, 2, 3, 4, 5, 6, 1.5f, 2.5f, [7, 8], 2, null, out _));
            Assert.Equal(1, scope.Server.Requests.Count - before);
            scope.Server.OnXml("/api/playresult2", "<Root />");
            Assert.Equal(ErrorReason.NetworkError, scope.Web.PlayResult("01", 1, 1, 1, 1, 1, 1, 1, 0, 1, "", null, out _));
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
