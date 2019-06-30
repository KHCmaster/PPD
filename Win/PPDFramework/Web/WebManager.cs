using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace PPDFramework.Web
{
    /// <summary>
    /// PPDWebへのマネージャークラスです。
    /// </summary>
    public class WebManager : PPDFrameworkCore.Web.WebManager
    {
        private const int Version = 6;

        private static WebManager webManager = new WebManager();
        private static AccountInfo currentAccount = new AccountInfo();

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        public static WebManager Instance
        {
            get
            {
                return webManager;
            }
        }

        /// <summary>
        /// ログインしているかどうかを取得します。
        /// </summary>
        public bool IsLogined
        {
            get
            {
                return currentAccount.IsValid;
            }
        }

        /// <summary>
        /// 現在のアカウントのIDを取得します。
        /// </summary>
        public string CurrentAccountId
        {
            get
            {
                return currentAccount.AccountId;
            }
        }

        /// <summary>
        /// 現在のアカウントのユーザー名を取得します。
        /// </summary>
        public string CurrentUserName
        {
            get
            {
                return currentAccount.UserName;
            }
        }

        private WebManager()
        {
        }

        /// <summary>
        /// ログインします。
        /// </summary>
        /// <param name="username">ユーザー名。</param>
        /// <param name="password">パスワード。</param>
        public ErrorReason Login(string username, string password)
        {
            var url = String.Format("{0}/api/login?username={1}&password={2}&version={3}", BaseUrl, username, password, Version);

            try
            {
                var req = CreateRequest(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                var result = reader.GetAttribute("Result");
                                if (result != "0")
                                {
                                    return (ErrorReason)int.Parse(result);
                                }
                                string accountId = reader.GetAttribute("AccountID"),
                                    nickname = reader.GetAttribute("Nickname"),
                                    token = reader.GetAttribute("Token"),
                                    ppdy = reader.GetAttribute("PPDY");
                                currentAccount = new AccountInfo(accountId, nickname, token, int.Parse(ppdy));
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                return ErrorReason.NetworkError;
            }

            return ErrorReason.OK;
        }

        /// <summary>
        /// ログアウトします
        /// </summary>
        public void Logout()
        {
            currentAccount = new AccountInfo();
        }

        /// <summary>
        /// アカウントの情報を更新します
        /// </summary>
        public void UpdateAccountInfo()
        {
            var data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return;
            }

            ErrorReason reason = ErrorReason.OK;
            var url = String.Format("{0}/api/update-account-info", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    data.Add(reader.Name, reader.Value);
                                }
                                if (!data.ContainsKey("Result"))
                                {
                                    reason = ErrorReason.NetworkError;
                                }
                                else
                                {
                                    if (int.TryParse(data["Result"], out int val))
                                    {
                                        reason = (ErrorReason)val;
                                    }
                                    else
                                    {
                                        reason = ErrorReason.NetworkError;
                                    }
                                }
                                if (reason == ErrorReason.OK)
                                {
                                    currentAccount.UserName = data["Nickname"];
                                    currentAccount.PPDY = int.Parse(data["PPDY"]);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                reason = ErrorReason.NetworkError;
            }
        }

        /// <summary>
        /// パーフェクトトライアルを行います。
        /// </summary>
        /// <param name="scoreHash">譜面のハッシュです。</param>
        /// <param name="data">データです。</param>
        /// <returns></returns>
        public ErrorReason PerfectTrialPrepare(byte[] scoreHash, out Dictionary<string, string> data)
        {
            return PerfectTrialPrepare(CryptographyUtility.Getx2Encoding(scoreHash), out data);
        }

        /// <summary>
        /// パーフェクトトライアルを行います。
        /// </summary>
        /// <param name="scoreHash">譜面のハッシュです。</param>
        /// <param name="data">データです。</param>
        /// <returns></returns>
        public ErrorReason PerfectTrialPrepare(string scoreHash, out Dictionary<string, string> data)
        {
            data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return ErrorReason.AuthFailed;
            }

            ErrorReason reason = ErrorReason.OK;
            var url = String.Format("{0}/api/perfect-trial-prepare", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "scorehash",scoreHash},
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    data.Add(reader.Name, reader.Value);
                                }
                                if (!data.ContainsKey("Result"))
                                {
                                    reason = ErrorReason.NetworkError;
                                }
                                else
                                {
                                    if (int.TryParse(data["Result"], out int val))
                                    {
                                        reason = (ErrorReason)val;
                                    }
                                    else
                                    {
                                        reason = ErrorReason.NetworkError;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                reason = ErrorReason.NetworkError;
            }

            return reason;

        }

        /// <summary>
        /// ゴーストを取得します。
        /// </summary>
        /// <param name="scoreHash"></param>
        /// <param name="maxCount"></param>
        /// <param name="ghosts"></param>
        /// <returns></returns>
        public ErrorReason GetGhost(string scoreHash, int maxCount, out GhostInfo[] ghosts)
        {
            if (!currentAccount.IsValid)
            {
                ghosts = new GhostInfo[0];
                return ErrorReason.AuthFailed;
            }

            ErrorReason reason = ErrorReason.OK;
            var url = String.Format("{0}/api/ghost", BaseUrl);
            var ret = new List<GhostInfo>();
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "scorehash",scoreHash},
                    { "maxcount",maxCount.ToString()}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                var result = int.Parse(reader.GetAttribute("Result"));
                                using (var subReader = reader.ReadSubtree())
                                {
                                    while (subReader.Read())
                                    {
                                        if (subReader.IsStartElement("Ghost"))
                                        {
                                            var id = int.Parse(subReader.GetAttribute("ID"));
                                            var accountId = subReader.GetAttribute("AccountID");
                                            var accountName = subReader.GetAttribute("AccountName");
                                            var showScore = subReader.GetAttribute("ShowScore") == "1";
                                            var showEvaluate = subReader.GetAttribute("ShowEvaluate") == "1";
                                            var showLife = subReader.GetAttribute("ShowLife") == "1";
                                            var data = subReader.ReadString();
                                            ret.Add(new GhostInfo(id, accountId, accountName,
                                                CryptographyUtility.GetBytesFromBase64String(data), showScore, showEvaluate, showLife));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                reason = ErrorReason.NetworkError;
            }

            ghosts = ret.ToArray();
            return reason;
        }

        /// <summary>
        /// プレイ結果を送信します
        /// </summary>
        /// <param name="scoreHash">スコアのハッシュ</param>
        /// <param name="score">スコア</param>
        /// <param name="coolCount">Coolの回数</param>
        /// <param name="goodCount">Goddの回数</param>
        /// <param name="safeCount">Safeの回数</param>
        /// <param name="sadCount">Sadの回数</param>
        /// <param name="worstCount">Worstの回数</param>
        /// <param name="maxCombo">MaxComboの回数</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">終了時間</param>
        /// <param name="inputs">入力データ</param>
        /// <param name="retryCount">リトライの回数</param>
        /// <param name="data">出力情報。</param>
        /// <param name="perfectTrialToken">トークン。</param>
        public ErrorReason PlayResult(byte[] scoreHash, int score, int coolCount, int goodCount, int safeCount, int sadCount, int worstCount,
            int maxCombo, float startTime, float endTime, byte[] inputs, int retryCount, string perfectTrialToken, out Dictionary<string, string> data)
        {
            data = new Dictionary<string, string>();
            ErrorReason reason = ErrorReason.OK;
            int tryCount = 0;
            while (tryCount <= retryCount)
            {
                reason = PlayResult(CryptographyUtility.Getx2Encoding(scoreHash), score, coolCount, goodCount, safeCount, sadCount, worstCount,
                    maxCombo, startTime, endTime, CryptographyUtility.GetBase64String(inputs), perfectTrialToken, out data);
                if (reason != ErrorReason.NetworkError)
                {
                    break;
                }
                tryCount++;
            }
            return reason;
        }

        /// <summary>
        /// プレイ結果を送信します
        /// </summary>
        /// <param name="scoreHash">スコアのハッシュ</param>
        /// <param name="score">スコア</param>
        /// <param name="coolCount">Coolの回数</param>
        /// <param name="goodCount">Goddの回数</param>
        /// <param name="safeCount">Safeの回数</param>
        /// <param name="sadCount">Sadの回数</param>
        /// <param name="worstCount">Worstの回数</param>
        /// <param name="maxCombo">MaxComboの回数</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">終了時間</param>
        /// <param name="inputs">入力データ</param>
        /// <param name="data">出力情報</param>
        /// <param name="perfectTrialToken">パーフェクトトライアルのトークン</param>
        public ErrorReason PlayResult(string scoreHash, int score, int coolCount, int goodCount, int safeCount, int sadCount, int worstCount, int maxCombo,
            float startTime, float endTime, string inputs, string perfectTrialToken, out Dictionary<string, string> data)
        {
            data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return ErrorReason.AuthFailed;
            }

            ErrorReason reason = ErrorReason.OK;
            var url = String.Format("{0}/api/playresult2", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "scorehash",scoreHash},
                    { "coolcount",coolCount.ToString()},
                    { "goodcount",goodCount.ToString()},
                    { "safecount",safeCount.ToString()},
                    { "sadcount",sadCount.ToString()},
                    { "worstcount",worstCount.ToString()},
                    { "maxcombo",maxCombo.ToString()},
                    { "score",score.ToString()},
                    { "inputs",inputs},
                    { "starttime",startTime.ToString(CultureInfo.InvariantCulture)},
                    { "endtime",endTime.ToString(CultureInfo.InvariantCulture)}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                query["perfecttrialtoken"] = perfectTrialToken;
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    data.Add(reader.Name, reader.Value);
                                }
                                if (!data.ContainsKey("Result"))
                                {
                                    reason = ErrorReason.NetworkError;
                                }
                                else
                                {
                                    if (int.TryParse(data["Result"], out int val))
                                    {
                                        reason = (ErrorReason)val;
                                    }
                                    else
                                    {
                                        reason = ErrorReason.NetworkError;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                reason = ErrorReason.NetworkError;
            }

            return reason;
        }

        /// <summary>
        /// ランキングを取得します
        /// </summary>
        /// <param name="scoreId">スコアのID</param>
        /// <returns></returns>
        public Ranking GetRanking(string scoreId)
        {
            var easy = new List<RankingInfo>();
            var normal = new List<RankingInfo>();
            var hard = new List<RankingInfo>();
            var extreme = new List<RankingInfo>();
            var url = String.Format("{0}/api/ranking?id={1}", BaseUrl, scoreId);
            try
            {
                var req = CreateRequest(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                if (reader.GetAttribute("Result") != "0")
                                {
                                    return null;
                                }
                            }
                            else if (reader.IsStartElement("Ranks"))
                            {
                                var difficulty = reader.GetAttribute("Difficulty");
                                List<RankingInfo> target = null;
                                switch (difficulty)
                                {
                                    case "0":
                                        target = easy;
                                        break;
                                    case "1":
                                        target = normal;
                                        break;
                                    case "2":
                                        target = hard;
                                        break;
                                    default:
                                        target = extreme;
                                        break;
                                }
                                using (XmlReader subReader = reader.ReadSubtree())
                                {
                                    while (subReader.Read())
                                    {
                                        if (subReader.IsStartElement("Rank"))
                                        {
                                            target.Add(new RankingInfo(subReader.GetAttribute("Nickname"), int.Parse(subReader.GetAttribute("Score")),
                                                subReader.GetAttribute("ID"), int.Parse(subReader.GetAttribute("Rank"))));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return new Ranking(easy.ToArray(), normal.ToArray(), hard.ToArray(), extreme.ToArray());
        }

        /// <summary>
        /// ランキングを取得します
        /// </summary>
        /// <param name="scoreId">スコアのID</param>
        /// <returns></returns>
        public Ranking GetRivalRanking(string scoreId)
        {
            if (!currentAccount.IsValid)
            {
                return new Ranking(new RankingInfo[0], new RankingInfo[0], new RankingInfo[0], new RankingInfo[0]);
            }

            var easy = new List<RankingInfo>();
            var normal = new List<RankingInfo>();
            var hard = new List<RankingInfo>();
            var extreme = new List<RankingInfo>();
            var url = String.Format("{0}/api/rival-ranking", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "scoreid",scoreId}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                if (reader.GetAttribute("Result") != "0")
                                {
                                    return null;
                                }
                            }
                            else if (reader.IsStartElement("Ranks"))
                            {
                                var difficulty = reader.GetAttribute("Difficulty");
                                List<RankingInfo> target = null;
                                switch (difficulty)
                                {
                                    case "0":
                                        target = easy;
                                        break;
                                    case "1":
                                        target = normal;
                                        break;
                                    case "2":
                                        target = hard;
                                        break;
                                    default:
                                        target = extreme;
                                        break;
                                }
                                using (XmlReader subReader = reader.ReadSubtree())
                                {
                                    while (subReader.Read())
                                    {
                                        if (subReader.IsStartElement("Rank"))
                                        {
                                            target.Add(new RankingInfo(subReader.GetAttribute("Nickname"), int.Parse(subReader.GetAttribute("Score")),
                                                subReader.GetAttribute("ID"), int.Parse(subReader.GetAttribute("Rank"))));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return new Ranking(easy.ToArray(), normal.ToArray(), hard.ToArray(), extreme.ToArray());
        }

        /// <summary>
        /// パーフェクトトライアルの一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public PerfectTrialInfo[] GetPerfectTrials()
        {
            var data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return new PerfectTrialInfo[0];
            }

            ErrorReason reason = ErrorReason.OK;
            var url = String.Format("{0}/api/get-perfect-trial-list", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                var trials = new List<PerfectTrialInfo>();
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    data.Add(reader.Name, reader.Value);
                                }
                                if (!data.ContainsKey("Result"))
                                {
                                    reason = ErrorReason.NetworkError;
                                }
                                else
                                {
                                    if (int.TryParse(data["Result"], out int val))
                                    {
                                        reason = (ErrorReason)val;
                                    }
                                    else
                                    {
                                        reason = ErrorReason.NetworkError;
                                    }
                                }
                                if (reason != ErrorReason.OK)
                                {
                                    return new PerfectTrialInfo[0];
                                }
                            }
                            else if (reader.IsStartElement("Trial"))
                            {
                                trials.Add(new PerfectTrialInfo(
                                    int.Parse(reader.GetAttribute("Id")),
                                    reader.GetAttribute("ScoreLibraryId"),
                                    reader.GetAttribute("ScoreHash"),
                                    (Difficulty)int.Parse(reader.GetAttribute("Difficulty"))));
                            }
                        }
                    }
                }
                return trials.ToArray();
            }
            catch
            {
                reason = ErrorReason.NetworkError;
            }

            return new PerfectTrialInfo[0];
        }

        /// <summary>
        /// リストの一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public ListInfo[] GetListInfos()
        {
            var data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return new ListInfo[0];
            }

            var url = String.Format("{0}/api/get-list-info", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                var lists = new List<ListInfo>();
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("List"))
                            {
                                var title = reader.GetAttribute("Title");
                                var isWatch = reader.GetAttribute("Watch") == "1";
                                var list = new ListInfo(title, isWatch);
                                lists.Add(list);
                                ReadListInfo(reader.ReadSubtree(), list);
                            }
                        }
                    }
                }
                return lists.ToArray();
            }
            catch
            {
                return new ListInfo[0];
            }
        }

        private void ReadListInfo(XmlReader reader, ListInfo listInfo)
        {
            using (reader)
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement("Score"))
                    {
                        var id = reader.GetAttribute("ID");
                        var title = reader.GetAttribute("Title");
                        listInfo.Add(new ListScoreInfo(listInfo, id, title));
                    }
                }
            }
        }

        /// <summary>
        /// 購入したリプレイの一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public ReplayInfo[] GetReplayInfos()
        {
            var data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return new ReplayInfo[0];
            }

            ErrorReason reason = ErrorReason.OK;
            var url = String.Format("{0}/api/get-replay-list", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                var replays = new List<ReplayInfo>();
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    data.Add(reader.Name, reader.Value);
                                }
                                if (!data.ContainsKey("Result"))
                                {
                                    reason = ErrorReason.NetworkError;
                                }
                                else
                                {
                                    if (int.TryParse(data["Result"], out int val))
                                    {
                                        reason = (ErrorReason)val;
                                    }
                                    else
                                    {
                                        reason = ErrorReason.NetworkError;
                                    }
                                }
                                if (reason != ErrorReason.OK)
                                {
                                    return new ReplayInfo[0];
                                }
                            }
                            else if (reader.IsStartElement("Purchase"))
                            {
                                replays.Add(new ReplayInfo(
                                    int.Parse(reader.GetAttribute("Id")),
                                    int.Parse(reader.GetAttribute("ResultId")),
                                    int.Parse(reader.GetAttribute("Score")),
                                    int.Parse(reader.GetAttribute("CoolCount")),
                                    int.Parse(reader.GetAttribute("GoodCount")),
                                    int.Parse(reader.GetAttribute("SafeCount")),
                                    int.Parse(reader.GetAttribute("SadCount")),
                                    int.Parse(reader.GetAttribute("WorstCount")),
                                    int.Parse(reader.GetAttribute("MaxCombo")),
                                    reader.GetAttribute("ScoreLibraryId"),
                                    reader.GetAttribute("ScoreHash"),
                                    reader.GetAttribute("Nickname")));
                            }
                        }
                    }
                }
                return replays.ToArray();
            }
            catch
            {
                reason = ErrorReason.NetworkError;
            }

            return new ReplayInfo[0];
        }

        /// <summary>
        /// リプレイデータを取得します。
        /// </summary>
        /// <param name="resultId">リザルトID。</param>
        /// <returns>リプレイデータ。</returns>
        public byte[] GetReplayData(int resultId)
        {
            var data = new Dictionary<string, string>();
            if (!currentAccount.IsValid)
            {
                return null;
            }

            var url = String.Format("{0}/api/get-replay", BaseUrl);
            try
            {
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "resultid",resultId.ToString()}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    var memoryStream = new MemoryStream();
                    byte[] buffer = new byte[1024];
                    using (var stream = res.GetResponseStream())
                    {
                        while (true)
                        {
                            var readSize = stream.Read(buffer, 0, buffer.Length);
                            if (readSize == 0)
                            {
                                break;
                            }
                            memoryStream.Write(buffer, 0, readSize);
                        }
                    }
                    return memoryStream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// スコアのIDを取得します
        /// </summary>
        /// <param name="scoreHash">スコアのハッシュ</param>
        /// <returns>スコアのID</returns>
        public string GetScoreId(byte[] scoreHash)
        {
            return GetScoreId(CryptographyUtility.Getx2Encoding(scoreHash));
        }

        /// <summary>
        /// スコアのIDを取得します
        /// </summary>
        /// <param name="scoreHash">スコアのハッシュ</param>
        /// <returns>スコアのID</returns>
        public string GetScoreId(string scoreHash)
        {
            var detail = GetScoreDetail(scoreHash);
            if (detail != null)
            {
                return detail.Id;
            }
            return "";
        }

        /// <summary>
        /// 譜面情報の詳細を取得します。
        /// </summary>
        /// <param name="scoreHash">スコアのハッシュ</param>
        /// <returns>譜面情報の詳細</returns>
        public WebSongInformationDetail GetScoreDetail(byte[] scoreHash)
        {
            return GetScoreDetail(CryptographyUtility.Getx2Encoding(scoreHash));
        }

        /// <summary>
        /// 譜面情報の詳細を取得します。
        /// </summary>
        /// <param name="scoreHash">スコアのハッシュ</param>
        /// <returns>譜面情報の詳細</returns>
        public WebSongInformationDetail GetScoreDetail(string scoreHash)
        {
            var url = String.Format("{0}/api/find-score-id?hash={1}",
               BaseUrl,
               scoreHash);

            try
            {
                var req = CreateRequest(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                if (reader.GetAttribute("Result") == "0")
                                {
                                    var id = reader.GetAttribute("ID");
                                    float startTime = float.Parse(reader.GetAttribute("StartTime"), NumberStyles.Float, CultureInfo.InvariantCulture),
                                        endTime = float.Parse(reader.GetAttribute("EndTime"), NumberStyles.Float, CultureInfo.InvariantCulture);
                                    return new WebSongInformationDetail(id, startTime, endTime);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private string CreateValidateKey(Dictionary<string, string> parameters)
        {
            var nonce = new Random().Next();
            var p = new SortedDictionary<string, string>(parameters)
            {
                { "nonce", nonce.ToString() },
                { "token", currentAccount.Token },
                { "accountid", currentAccount.AccountId }
            };
            return CreateValidateKey(p);
        }

        private string CreateValidateKey(SortedDictionary<string, string> parameters)
        {
            return "";
        }

        /// <summary>
        /// アイテムの一覧を取得します。
        /// </summary>
        /// <returns>アイテムの一覧。</returns>
        public ItemInfo[] GetItems()
        {
            if (!currentAccount.IsValid)
            {
                return new ItemInfo[0];
            }

            var ret = new List<ItemInfo>();
            try
            {
                var url = String.Format("{0}/api/get-item-list", BaseUrl);
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreateRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                if (reader.GetAttribute("Result") != "0")
                                {
                                    break;
                                }
                                using (XmlReader subtree = reader.ReadSubtree())
                                {
                                    while (subtree.Read())
                                    {
                                        if (reader.IsStartElement("Item"))
                                        {
                                            var itemId = int.Parse(reader.GetAttribute("ID"));
                                            var itemType = (ItemType)int.Parse(reader.GetAttribute("Type"));
                                            ret.Add(new ItemInfo(itemId, itemType));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return ret.ToArray();
        }

        /// <summary>
        /// アイテムを使用します。
        /// </summary>
        /// <param name="itemInfo">アイテムの情報。</param>
        /// <returns>成功したかどうか。</returns>
        public bool UseItem(ItemInfo itemInfo)
        {
            if (!currentAccount.IsValid)
            {
                return false;
            }

            bool ret = false;
            try
            {
                var url = String.Format("{0}/api/use-item", BaseUrl);
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "id",itemInfo.ItemId.ToString()}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                ret = reader.GetAttribute("Result") == "0";
                            }
                        }
                    }
                }
            }
            catch
            {
                ret = false;
            }
            itemInfo.IsUsed |= ret;
            return ret;
        }

        /// <summary>
        /// レビューをします。
        /// </summary>
        /// <param name="comment">レビュー文字列。</param>
        /// <param name="score">レート。</param>
        /// <param name="scoreHash">スコアのハッシュ。</param>
        /// <returns></returns>
        public bool Review(string comment, int score, byte[] scoreHash)
        {
            if (!currentAccount.IsValid)
            {
                return false;
            }

            bool ret = false;

            try
            {
                var url = String.Format("{0}/api/review", BaseUrl);
                var query = new SortedDictionary<string, string>{
                    {"accountid", currentAccount.AccountId},
                    { "nonce", new Random().Next().ToString()},
                    { "token",currentAccount.Token},
                    { "scorehash",CryptographyUtility.Getx2Encoding(scoreHash)},
                    { "comment",comment},
                    { "score",score.ToString()}
                };
                var validateKey = CreateValidateKey(query);
                query["validatekey"] = validateKey;
                query.Remove("token");
                var req = CreatePostRequest(url, query);
                using (WebResponse res = req.GetResponse())
                {
                    using (XmlReader reader = GetXmlReader(res))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Root"))
                            {
                                ret = reader.GetAttribute("Result") == "0";
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return ret;
        }

        /// <summary>
        /// ランキングが利用可能な譜面かどうかを調べます。
        /// </summary>
        /// <param name="primaryHash">プライマリハッシュ。</param>
        /// <returns></returns>
        public bool IsRankingAvailable(byte[] primaryHash)
        {
            return RankingCache.Cache.IsRankingAvailable(primaryHash);
        }

        /// <summary>
        /// Webに登録されている譜面情報を取得します。
        /// </summary>
        /// <returns></returns>
        public WebSongInformation[] GetScores()
        {
            return GetScores(null);
        }

        /// <summary>
        /// Webに登録されている譜面情報を取得します。
        /// </summary>
        /// <returns></returns>
        public WebSongInformation[] GetScores(bool onlyActiveScore)
        {
            var activeScores = GetActiveScores();
            var indexes = new Dictionary<string, int>();
            var iter = 0;
            foreach (var activeScore in activeScores)
            {
                indexes.Add(activeScore.Hash, iter);
                iter++;
            }
            var list = GetScores(s => activeScores.FirstOrDefault(active => active.Hash == s.Hash) != null);
            Array.Sort(list, (w1, w2) =>
            {
                return indexes[w1.Hash] - indexes[w2.Hash];
            });
            return list;
        }

        private WebSongInformation[] GetScores(Func<WebSongInformation, bool> filter)
        {
            var list = new List<WebSongInformation>();
            try
            {
                var str = GetString(String.Format("{0}/api/all-score-list", BaseUrl));
                using (XmlReader reader = GetXmlReader(str))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Score"))
                        {
                            string title = reader.GetAttribute("Title"),
                                hash = reader.GetAttribute("Hash"),
                                movieUrl = reader.GetAttribute("MovieUrl"),
                                revision = reader.GetAttribute("Revision");

                            var info = new WebSongInformation(title, hash, movieUrl, int.Parse(revision));
                            if (filter != null && !filter(info))
                            {
                                continue;
                            }
                            list.Add(info);
                            ParseDifficulty(reader.ReadSubtree(), info);
                            info.CheckNewOrCanUpdate();
                        }
                    }
                }
            }
            catch
            {
            }
            return list.ToArray();
        }

        private void ParseDifficulty(XmlReader reader, WebSongInformation songInfo)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement("ScoreDifficulty"))
                {
                    string difficulty = reader.GetAttribute("Difficulty"),
                        hash = reader.GetAttribute("Hash"),
                        revision = reader.GetAttribute("Revision");
                    songInfo.AddDifficulty(new WebSongInformationDifficulty(songInfo, (Difficulty)int.Parse(difficulty), int.Parse(revision), hash));
                }
            }
            reader.Close();
        }

        /// <summary>
        /// Webに登録されているランキング対象譜面の一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public WebSongInformation[] GetActiveScores()
        {
            var list = new List<WebSongInformation>();
            try
            {
                var str = GetString(String.Format("{0}/api/active-score", BaseUrl));
                using (XmlReader reader = GetXmlReader(str))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Entry"))
                        {
                            string title = reader.GetAttribute("Title"),
                                hash = reader.GetAttribute("ID");

                            var info = new WebSongInformation(title, hash, null, -1);
                            list.Add(info);
                        }
                    }
                }
            }
            catch
            {
            }
            return list.ToArray();
        }

        /// <summary>
        /// Webに登録されているMod情報を取得します。
        /// </summary>
        /// <returns></returns>
        public WebModInfo[] GetMods()
        {
            var list = new List<WebModInfo>();
            try
            {
                var str = GetString(String.Format("{0}/api/all-mod-list", BaseUrl));
                using (XmlReader reader = GetXmlReader(str))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Mod"))
                        {
                            string name = reader.GetAttribute("Name"),
                                id = reader.GetAttribute("ID"),
                            revision = reader.GetAttribute("Revision");

                            var info = new WebModInfo(name, id, int.Parse(revision));
                            list.Add(info);
                            ParseDetail(reader.ReadSubtree(), info);
                        }
                    }
                }
            }
            catch
            {
            }
            return list.ToArray();
        }

        private void ParseDetail(XmlReader reader, WebModInfo modInfo)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement("ModFile"))
                {
                    string hash = reader.GetAttribute("Hash"),
                        revision = reader.GetAttribute("Revision");
                    modInfo.AddDetail(new WebModInfoDetail(modInfo, hash, int.Parse(revision)));
                }
            }
            reader.Close();
        }

        /// <summary>
        /// 譜面ページのURLを取得します。
        /// </summary>
        /// <param name="scoreLibraryId">スコアのID。</param>
        /// <returns>URL。</returns>
        public string GetScorePageUrl(string scoreLibraryId)
        {
            return String.Format("{0}/score/index/id/{1}", BaseUrl, scoreLibraryId);
        }

        /// <summary>
        /// アカウントの画像を取得します。
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public byte[] GetAccountImage(string accountId, int size = 128)
        {
            var url = String.Format("{0}/api/get-avator/s/{1}/id/{2}", PPDFrameworkCore.Web.WebManager.BaseUrl, size, accountId);
            try
            {
                var req = CreateRequest(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        using (Stream stream = res.GetResponseStream())
                        {
                            while (true)
                            {
                                var readSize = stream.Read(buffer, 0, buffer.Length);
                                if (readSize == 0)
                                {
                                    break;
                                }

                                memoryStream.Write(buffer, 0, readSize);
                            }
                        }

                        return memoryStream.ToArray();
                    }
                }
            }
            catch
            {
            }

            return new byte[0];
        }

        /// <summary>
        /// Webからファイルをダウンロードします。
        /// </summary>
        /// <param name="songInfo">ウェブの譜面情報。</param>
        /// <param name="filePath">ファイルパス。</param>
        public void DownloadFile(WebSongInformation songInfo, string filePath)
        {
            DownloadFile(songInfo, filePath, BaseUrl);
        }

        /// <summary>
        /// Webからファイルをダウンロードします。
        /// </summary>
        /// <param name="songInfo">ウェブの譜面情報。</param>
        /// <param name="filePath">ファイルパス。</param>
        /// <param name="BaseUrl">既定のホスト。</param>
        public void DownloadFile(WebSongInformation songInfo, string filePath, string BaseUrl)
        {
            DownloadFile(String.Format("{0}/score-library/download/id/{1}", BaseUrl, songInfo.Hash), filePath);
        }

        /// <summary>
        /// Webからファイルをダウンロードします。
        /// </summary>
        /// <param name="modInfo">WebのMod情報。</param>
        /// <param name="filePath">ファイルパス。</param>
        public void DownloadFile(WebModInfo modInfo, string filePath)
        {
            DownloadFile(modInfo, filePath, BaseUrl);
        }

        /// <summary>
        /// Webからファイルをダウンロードします。
        /// </summary>
        /// <param name="modInfo">WebのMod情報。</param>
        /// <param name="filePath">ファイルパス。</param>
        /// <param name="BaseUrl">既定のホスト。</param>
        public void DownloadFile(WebModInfo modInfo, string filePath, string BaseUrl)
        {
            DownloadFile(String.Format("{0}/script-library/download/id/{1}", BaseUrl, modInfo.Id), filePath);
        }

        /// <summary>
        /// Webからファイルをダウンロードします。
        /// </summary>
        /// <param name="url">URL。</param>
        /// <param name="filePath">ファイルパス。</param>
        public void DownloadFile(string url, string filePath)
        {
            var req = CreateRequest(url);
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (FileStream fs = File.Open(filePath, FileMode.Create))
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        byte[] buffer = new byte[512];
                        while (true)
                        {
                            var readSize = stream.Read(buffer, 0, buffer.Length);
                            if (readSize == 0)
                            {
                                break;
                            }

                            fs.Write(buffer, 0, readSize);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// コンテストの情報を取得します。
        /// </summary>
        /// <returns>コンテストの情報。</returns>
        public ContestInfo GetContestInfo()
        {
            try
            {
                var str = GetString(String.Format("{0}/api/get-latest-contest", BaseUrl));
                using (XmlReader reader = GetXmlReader(str))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Root"))
                        {
                            var info = new ContestInfo(int.Parse(reader.GetAttribute("Id")), reader.GetAttribute("ScoreLibraryId"), reader.GetAttribute("ScoreHash"),
                                (Difficulty)int.Parse(reader.GetAttribute("Difficulty")), ParseDateString(reader.GetAttribute("StartTime")),
                                ParseDateString(reader.GetAttribute("CurrentTime")), ParseDateString(reader.GetAttribute("EndTime")), reader.GetAttribute("Title"));

                            return info;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// コンテストのランキングを取得します。
        /// </summary>
        /// <returns>コンテストのランキング。</returns>
        public RankingInfo[] GetContestRanking()
        {
            var ret = new List<RankingInfo>();
            try
            {
                var str = GetString(String.Format("{0}/api/get-latest-contest-result", BaseUrl));
                using (XmlReader reader = GetXmlReader(str))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Result"))
                        {
                            ret.Add(new RankingInfo(reader.GetAttribute("Nickname"), int.Parse(reader.GetAttribute("Score")),
                                reader.GetAttribute("AccountId"), int.Parse(reader.GetAttribute("Rank"))));
                        }
                    }
                }
            }
            catch
            {
            }
            return ret.ToArray();
        }

        private DateTime ParseDateString(string str)
        {
            return DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private HttpWebRequest CreateRequest(string url, IDictionary<string, string> parameters)
        {
            var paramList = new List<string>();
            foreach (KeyValuePair<string, string> kvp in parameters)
            {
                paramList.Add(String.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value)));
            }
            return CreateRequest(String.Format("{0}?{1}", url, String.Join("&", paramList.ToArray())));
        }

        private HttpWebRequest CreateRequest(string url)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 15000;
#if DEBUG
            if (File.Exists("credentials"))
            {
                var lines = File.ReadAllLines("credentials");
                if (lines.Length >= 2)
                {
                    request.Credentials = new NetworkCredential(lines[0], lines[1]);
                }
            }
#endif
            return request;
        }

        private string GetString(string url)
        {
            var req = CreateRequest(url);
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private HttpWebRequest CreatePostRequest(string url, IDictionary<string, string> param)
        {
            var paramList = new List<string>();
            foreach (KeyValuePair<string, string> kvp in param)
            {
                paramList.Add(String.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value)));
            }

            var postData = String.Join("&", paramList.ToArray());
            var postDataBytes = System.Text.Encoding.ASCII.GetBytes(postData);

            var req = CreateRequest(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = postDataBytes.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            }
            return req;
        }

        private XmlReader GetXmlReader(WebResponse res, bool dump = false)
        {
            if (dump)
            {
                string text = null;
                using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                    Console.WriteLine(text);
                }
                var memStream = new MemoryStream();
                var bytes = Encoding.UTF8.GetBytes(text);
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return XmlReader.Create(memStream);
            }
            else
            {
                return XmlReader.Create(res.GetResponseStream());
            }
        }

        private XmlReader GetXmlReader(string text)
        {

#if DUMP
            Console.WriteLine(text);
#endif
            var memStream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(text);
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return XmlReader.Create(memStream);

        }
    }
}
