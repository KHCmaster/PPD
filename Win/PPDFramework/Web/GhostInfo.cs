namespace PPDFramework.Web
{
    /// <summary>
    /// ゴーストの情報です。
    /// </summary>
    public class GhostInfo
    {
        /// <summary>
        /// IDを取得します。
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// アカウントIDを取得します。
        /// </summary>
        public string AccountId
        {
            get;
            private set;
        }

        /// <summary>
        /// アカウント名を取得します。
        /// </summary>
        public string AccountName
        {
            get;
            private set;
        }

        /// <summary>
        /// リプレイデータを取得します。
        /// </summary>
        public byte[] ReplayData
        {
            get;
            private set;
        }

        /// <summary>
        /// スコアを表示します。
        /// </summary>
        public bool ShowScore
        {
            get;
            private set;
        }

        /// <summary>
        /// 評価を表示します。
        /// </summary>
        public bool ShowEvaluate
        {
            get;
            private set;
        }

        /// <summary>
        /// ライフを表示します。
        /// </summary>
        public bool ShowLife
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="replayData"></param>
        /// <param name="showScore"></param>
        /// <param name="showEvaluate"></param>
        /// <param name="showLife"></param>
        public GhostInfo(int id, string accountId, string accountName, byte[] replayData, bool showScore, bool showEvaluate, bool showLife)
        {
            Id = id;
            AccountId = accountId;
            AccountName = accountName;
            ReplayData = replayData;
            ShowScore = showScore;
            ShowEvaluate = showEvaluate;
            ShowLife = showLife;
        }
    }
}
