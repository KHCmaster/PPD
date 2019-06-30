namespace PPDFramework.Web
{
    /// <summary>
    /// ランキングの詳細のクラスです
    /// </summary>
    public class RankingInfo
    {
        internal RankingInfo(string nickname, int score, string id, int rank)
        {
            Nickname = nickname;
            Score = score;
            ID = id;
            Rank = rank;
        }

        /// <summary>
        /// ニックネームを取得します
        /// </summary>
        public string Nickname
        {
            get;
            private set;
        }

        /// <summary>
        /// スコアを取得します
        /// </summary>
        public int Score
        {
            get;
            private set;
        }

        /// <summary>
        /// IDを取得します
        /// </summary>
        public string ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 順位を取得します
        /// </summary>
        public int Rank
        {
            get;
            private set;
        }
    }
}
