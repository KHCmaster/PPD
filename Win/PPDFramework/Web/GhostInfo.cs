namespace PPDFramework.Web
{
    /// <summary>
    /// ゴーストの情報です。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="accountId"></param>
    /// <param name="accountName"></param>
    /// <param name="replayData"></param>
    /// <param name="showScore"></param>
    /// <param name="showEvaluate"></param>
    /// <param name="showLife"></param>
    public class GhostInfo(int id, string accountId, string accountName, byte[] replayData, bool showScore, bool showEvaluate, bool showLife)
    {
        /// <summary>
        /// IDを取得します。
        /// </summary>
        public int Id
        {
            get;
            private set;
        } = id;

        /// <summary>
        /// アカウントIDを取得します。
        /// </summary>
        public string AccountId
        {
            get;
            private set;
        } = accountId;

        /// <summary>
        /// アカウント名を取得します。
        /// </summary>
        public string AccountName
        {
            get;
            private set;
        } = accountName;

        /// <summary>
        /// リプレイデータを取得します。
        /// </summary>
        public byte[] ReplayData
        {
            get;
            private set;
        } = replayData;

        /// <summary>
        /// スコアを表示します。
        /// </summary>
        public bool ShowScore
        {
            get;
            private set;
        } = showScore;

        /// <summary>
        /// 評価を表示します。
        /// </summary>
        public bool ShowEvaluate
        {
            get;
            private set;
        } = showEvaluate;

        /// <summary>
        /// ライフを表示します。
        /// </summary>
        public bool ShowLife
        {
            get;
            private set;
        } = showLife;
    }
}
