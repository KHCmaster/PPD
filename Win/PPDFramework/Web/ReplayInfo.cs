namespace PPDFramework.Web
{
    /// <summary>
    /// リプレイの情報を扱うクラスです。
    /// </summary>
    public class ReplayInfo
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
        /// リザルトIDを取得します。
        /// </summary>
        public int ResultId
        {
            get;
            private set;
        }

        /// <summary>
        /// スコアを取得します。
        /// </summary>
        public int Score
        {
            get;
            private set;
        }

        /// <summary>
        /// COOL数を取得します。
        /// </summary>
        public int CoolCount
        {
            get;
            private set;
        }

        /// <summary>
        /// GOOD数を取得します。
        /// </summary>
        public int GoodCount
        {
            get;
            private set;
        }

        /// <summary>
        /// SAFE数を取得します。
        /// </summary>
        public int SafeCount
        {
            get;
            private set;
        }

        /// <summary>
        /// SAD数を取得します。
        /// </summary>
        public int SadCount
        {
            get;
            private set;
        }

        /// <summary>
        /// WORST数を取得します。
        /// </summary>
        public int WorstCount
        {
            get;
            private set;
        }

        /// <summary>
        /// MAXCOMBO数を取得します。
        /// </summary>
        public int MaxCombo
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面IDを取得します。
        /// </summary>
        public string ScoreLibraryId
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面ハッシュを取得します。
        /// </summary>
        public string ScoreHash
        {
            get;
            private set;
        }

        /// <summary>
        /// ニックネームを取得します。
        /// </summary>
        public string Nickname
        {
            get;
            private set;
        }

        internal ReplayInfo(int id, int resultId, int score, int coolCount, int goodCount, int safeCount, int sadCount, int worstCount,
            int maxCombo, string scoreLibraryId, string scoreHash, string nickname)
        {
            Id = id;
            ResultId = resultId;
            Score = score;
            CoolCount = coolCount;
            GoodCount = goodCount;
            SafeCount = safeCount;
            SadCount = sadCount;
            WorstCount = worstCount;
            MaxCombo = maxCombo;
            ScoreLibraryId = scoreLibraryId;
            ScoreHash = scoreHash;
            Nickname = nickname;
        }
    }
}
