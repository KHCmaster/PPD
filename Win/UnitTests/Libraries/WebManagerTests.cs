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
        public void AccountLists_NeedLoginAndStopAtErrors()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/get-perfect-trial-list", """
                <Root Result="0"><Trial Id="1" ScoreLibraryId="lib1" ScoreHash="h1" Difficulty="2" /><Trial Id="2" ScoreLibraryId="lib2" ScoreHash="h2" Difficulty="3" /></Root>
                """);
            scope.Server.OnXml("/api/get-list-info", """
                <Root><List Title="好き" Watch="1"><Score ID="s1" Title="曲1" /><Score ID="s2" Title="曲2" /></List><List Title="あとで" Watch="0" /></Root>
                """);
            scope.Server.OnXml("/api/get-replay-list", """
                <Root Result="0"><Purchase Id="5" ResultId="50" Score="1000" CoolCount="1" GoodCount="2" SafeCount="3" SadCount="4" WorstCount="5" MaxCombo="6" ScoreLibraryId="lib" ScoreHash="hash" Nickname="nick" /></Root>
                """);
            scope.Server.On("/api/get-replay", request => new TestWebServer.Response(HttpStatusCode.OK, [9, 8, (byte)int.Parse(request.Form["resultid"])]));

            // ログインしていなければ送らない
            Assert.Empty(scope.Web.GetPerfectTrials());
            Assert.Empty(scope.Web.GetListInfos());
            Assert.Empty(scope.Web.GetReplayInfos());
            Assert.Null(scope.Web.GetReplayData(7));
            Assert.Empty(scope.Server.Requests);

            Login(scope);
            var trials = scope.Web.GetPerfectTrials();
            Assert.Equal([1, 2], trials.Select(t => t.Id));
            Assert.Equal(("lib1", "h1", PPDFrameworkCore.Difficulty.Hard), (trials[0].ScoreLibraryId, trials[0].ScoreHash, trials[0].Difficulty));

            var lists = scope.Web.GetListInfos();
            Assert.Equal([("好き", true), ("あとで", false)], lists.Select(l => (l.Title, l.IsWatch)));
            Assert.Equal([("s1", "曲1"), ("s2", "曲2")], lists[0].Scores.Select(s => (s.ScoreId, s.Title)));
            Assert.Same(lists[0], lists[0].Scores[0].Parent);
            Assert.Empty(lists[1].Scores);

            var replay = scope.Web.GetReplayInfos().Single();
            Assert.Equal((5, 50, 1000, 1, 2, 3, 4, 5, 6), (replay.Id, replay.ResultId, replay.Score, replay.CoolCount, replay.GoodCount, replay.SafeCount, replay.SadCount, replay.WorstCount, replay.MaxCombo));
            Assert.Equal(("lib", "hash", "nick"), (replay.ScoreLibraryId, replay.ScoreHash, replay.Nickname));
            Assert.Equal([9, 8, 7], scope.Web.GetReplayData(7));

            // 失敗の結果、結果のない応答、通信のエラー
            scope.Server.OnXml("/api/get-perfect-trial-list", """<Root Result="1"><Trial Id="1" ScoreLibraryId="l" ScoreHash="h" Difficulty="0" /></Root>""");
            scope.Server.OnXml("/api/get-replay-list", "<Root />");
            Assert.Empty(scope.Web.GetPerfectTrials());
            Assert.Empty(scope.Web.GetReplayInfos());
            scope.Server.OnXml("/api/get-perfect-trial-list", """<Root Result="x" />""");
            scope.Server.OnXml("/api/get-replay-list", """<Root Result="x" />""");
            Assert.Empty(scope.Web.GetPerfectTrials());
            Assert.Empty(scope.Web.GetReplayInfos());
            scope.Server.OnXml("/api/get-replay-list", """<Root Result="2" />""");
            Assert.Empty(scope.Web.GetReplayInfos());
            foreach (var path in new[] { "/api/get-perfect-trial-list", "/api/get-list-info", "/api/get-replay-list", "/api/get-replay" })
            {
                scope.Server.On(path, _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            }
            Assert.Empty(scope.Web.GetPerfectTrials());
            Assert.Empty(scope.Web.GetListInfos());
            Assert.Empty(scope.Web.GetReplayInfos());
            Assert.Null(scope.Web.GetReplayData(7));
        }

        [Fact]
        public void Items_ListUseAndReview()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/get-item-list", """<Root Result="0"><Item ID="1" Type="0" /><Item ID="2" Type="1" /></Root>""");
            scope.Server.OnXml("/api/use-item", """<Root Result="0" />""");
            scope.Server.OnXml("/api/review", """<Root Result="0" />""");
            var item = new ItemInfo(1, 0);
            Assert.Empty(scope.Web.GetItems());
            Assert.False(scope.Web.UseItem(item));
            Assert.False(scope.Web.Review("good", 5, [1]));
            Assert.Empty(scope.Server.Requests);

            Login(scope);
            var items = scope.Web.GetItems();
            Assert.Equal([1, 2], items.Select(i => i.ItemId));
            Assert.Equal((ItemType)1, items[1].ItemType);
            // アイテムの一覧は GET で送る
            Assert.Equal(("GET", "acc1"), (scope.Server.Requests.Last().Method, scope.Server.Requests.Last().Query["accountid"]));

            Assert.True(scope.Web.UseItem(item));
            Assert.True(item.IsUsed);
            Assert.Equal("1", scope.Server.Requests.Last().Form["id"]);
            Assert.True(scope.Web.Review("よい曲", 5, [0xab]));
            Assert.Equal(("よい曲", "5", "ab"), (scope.Server.Requests.Last().Form["comment"], scope.Server.Requests.Last().Form["score"], scope.Server.Requests.Last().Form["scorehash"]));

            scope.Server.OnXml("/api/get-item-list", """<Root Result="1"><Item ID="1" Type="0" /></Root>""");
            scope.Server.OnXml("/api/use-item", """<Root Result="1" />""");
            scope.Server.OnXml("/api/review", """<Root Result="1" />""");
            Assert.Empty(scope.Web.GetItems());
            var other = new ItemInfo(2, 0);
            Assert.False(scope.Web.UseItem(other));
            Assert.False(other.IsUsed);
            Assert.False(scope.Web.Review("x", 1, [1]));
            scope.Server.On("/api/use-item", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            scope.Server.On("/api/review", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.False(scope.Web.UseItem(other));
            Assert.False(scope.Web.Review("x", 1, [1]));
        }

        [Fact]
        public void Scores_AreMatchedWithLocalSongs()
        {
            using var database = new SongDatabase();
            database.AddSong("local", "guid", 120, ("Easy", null));
            PPDFramework.SongInformation.Update();
            var local = SongDatabase.Song("local");
            var localHash = PPDFrameworkCore.CryptographyUtility.Getx2Encoding(local.EasyHash);
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/all-score-list", $"""
                <Root>
                  <Score Title="新しい" Hash="new" MovieUrl="http://movie" Revision="1"><ScoreDifficulty Difficulty="1" Hash="00ff" Revision="1" /></Score>
                  <Score Title="持っている" Hash="have" MovieUrl="" Revision="3"><ScoreDifficulty Difficulty="0" Hash="{localHash}" Revision="2" /></Score>
                </Root>
                """);
            scope.Server.OnXml("/api/active-score", """<Root><Entry Title="持っている" ID="have" /><Entry Title="新しい" ID="new" /></Root>""");

            var scores = scope.Web.GetScores();

            Assert.Equal(["new", "have"], scores.Select(s => s.Hash));
            Assert.Equal(("新しい", "http://movie", 1), (scores[0].Title, scores[0].MovieUrl, scores[0].Revision));
            // 持っていない譜面は新しい。持っている譜面は、Web の版が新しければ更新できる
            Assert.True(scores[0].IsNew);
            Assert.False(scores[1].IsNew);
            Assert.True(scores[1].CanUpdate);
            Assert.Same(local, scores[1].GetSongInformation());
            Assert.Null(scores[0].GetSongInformation());
            var difficulty = scores[1].Difficulties.Single();
            Assert.Equal((PPDFrameworkCore.Difficulty.Easy, 2, localHash), (difficulty.Difficulty, difficulty.Revision, difficulty.Hash));
            Assert.Equal(local.EasyHash, difficulty.HashAsBytes);

            // ランキングの対象の譜面だけを、対象の順に
            Assert.Equal(["have", "new"], scope.Web.GetScores(true).Select(s => s.Hash));
            Assert.Equal([("持っている", "have", -1), ("新しい", "new", -1)], scope.Web.GetActiveScores().Select(s => (s.Title, s.Hash, s.Revision)));

            scope.Server.On("/api/all-score-list", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            scope.Server.On("/api/active-score", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Empty(scope.Web.GetScores());
            Assert.Empty(scope.Web.GetActiveScores());
        }

        [Fact]
        public void Mods_AndDownloads()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/all-mod-list", """<Root><Mod Name="mod" ID="m1" Revision="2"><ModFile Hash="0102" Revision="1" /><ModFile Hash="0304" Revision="2" /></Mod></Root>""");
            scope.Server.On("/score-library/download/id/song1", _ => new TestWebServer.Response(HttpStatusCode.OK, [1, 2, 3]));
            scope.Server.On("/script-library/download/id/m1", _ => new TestWebServer.Response(HttpStatusCode.OK, [4, 5]));
            scope.Server.On("/api/get-avator/s/64/id/acc1", _ => new TestWebServer.Response(HttpStatusCode.OK, [6]));

            var mod = scope.Web.GetMods().Single();
            Assert.Equal(("mod", "m1", 2), (mod.Name, mod.Id, mod.Revision));
            Assert.Equal([1, 2], mod.Details.Select(d => d.Revision));
            Assert.Equal([3, 4], mod.Details[1].HashAsBytes);
            Assert.Same(mod, mod.Details[0].WebModInfo);

            scope.Web.DownloadFile(new WebSongInformation("t", "song1", "", 1), "song.zip");
            scope.Web.DownloadFile(mod, "mod.zip");
            Assert.Equal([1, 2, 3], System.IO.File.ReadAllBytes("song.zip"));
            Assert.Equal([4, 5], System.IO.File.ReadAllBytes("mod.zip"));
            Assert.Equal([6], scope.Web.GetAccountImage("acc1", 64));
            // ない画像は空
            Assert.Empty(scope.Web.GetAccountImage("none"));
            Assert.Equal(scope.Server.BaseUrl + "/score/index/id/lib1", scope.Web.GetScorePageUrl("lib1"));

            scope.Server.On("/api/all-mod-list", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Empty(scope.Web.GetMods());
            Assert.ThrowsAny<System.Net.Http.HttpRequestException>(() => scope.Web.DownloadFile(scope.Server.BaseUrl + "/none", "none.zip"));
        }

        [Fact]
        public void Contest_InfoAndRanking()
        {
            using var scope = new WebTestScope();
            scope.Server.OnXml("/api/get-latest-contest", """
                <Root Id="3" ScoreLibraryId="lib" ScoreHash="hash" Difficulty="3" StartTime="2026-01-02 03:04:05" CurrentTime="2026-01-03 00:00:00" EndTime="2026-02-01 12:00:00" Title="大会" />
                """);
            scope.Server.OnXml("/api/get-latest-contest-result", """<Root><Result Nickname="a" Score="10" AccountId="x" Rank="1" /><Result Nickname="b" Score="5" AccountId="y" Rank="2" /></Root>""");

            var contest = scope.Web.GetContestInfo();
            Assert.Equal((3, "lib", "hash", PPDFrameworkCore.Difficulty.Extreme, "大会"), (contest.Id, contest.ScoreLibraryId, contest.ScoreHash, contest.Difficulty, contest.Title));
            Assert.Equal((new System.DateTime(2026, 1, 2, 3, 4, 5), new System.DateTime(2026, 2, 1, 12, 0, 0)), (contest.StartTime, contest.EndTime));
            Assert.Equal([("a", 10, "x", 1), ("b", 5, "y", 2)], scope.Web.GetContestRanking().Select(r => (r.Nickname, r.Score, r.ID, r.Rank)));

            // 日時の形が違う、コンテストがない
            scope.Server.OnXml("/api/get-latest-contest", """<Root Id="3" ScoreLibraryId="lib" ScoreHash="hash" Difficulty="3" StartTime="yesterday" CurrentTime="" EndTime="" Title="" />""");
            Assert.Null(scope.Web.GetContestInfo());
            scope.Server.OnXml("/api/get-latest-contest", "<None />");
            Assert.Null(scope.Web.GetContestInfo());
            scope.Server.On("/api/get-latest-contest-result", _ => TestWebServer.Status(HttpStatusCode.InternalServerError));
            Assert.Empty(scope.Web.GetContestRanking());
        }

        [Fact]
        public void RankingCache_KeepsRankingForAMinute()
        {
            using var scope = new WebTestScope();
            // ほかのテストのキャッシュと重ならない譜面
            byte[] hash = [.. System.Guid.NewGuid().ToByteArray()];
            var hex = PPDFrameworkCore.CryptographyUtility.Getx2Encoding(hash);
            scope.Server.On("/api/find-score-id", request => request.Query["hash"] == hex
                ? TestWebServer.Xml("""<Root Result="0" ID="cached" StartTime="0" EndTime="1" />""")
                : TestWebServer.Xml("""<Root Result="1" />"""));
            scope.Server.OnXml("/api/ranking", RankingXml);
            scope.Server.OnXml("/api/rival-ranking", RankingXml);
            Assert.False(scope.Web.IsRankingAvailable(hash));

            var ranking = RankingCache.Cache.GetRanking(hash);
            Assert.Equal(2, ranking.Easy.Length);
            Assert.True(scope.Web.IsRankingAvailable(hash));
            // 2 回目はキャッシュから（送らない）
            var count = scope.Server.Requests.Count;
            Assert.Same(ranking, RankingCache.Cache.GetRanking(hash));
            Assert.Equal(count, scope.Server.Requests.Count);
            // 強制すると取り直す
            Assert.NotSame(ranking, RankingCache.Cache.GetRanking(hash, forceUpdate: true));

            Login(scope);
            var rival = RankingCache.Cache.GetRivalRanking(hash);
            Assert.Equal(2, rival.Easy.Length);
            Assert.Same(rival, RankingCache.Cache.GetRivalRanking(hash));
            Assert.NotSame(rival, RankingCache.Cache.GetRivalRanking(hash, forceUpdate: true));

            // Web にない譜面
            byte[] unknown = [.. System.Guid.NewGuid().ToByteArray()];
            Assert.Null(RankingCache.Cache.GetRanking(unknown));
            Assert.Null(RankingCache.Cache.GetRivalRanking(unknown));
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
