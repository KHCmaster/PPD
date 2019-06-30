using PPDFrameworkCore;

namespace PPDFramework.Web
{
    /// <summary>
    /// パーフェクトトライアルの情報を表すクラスです。
    /// </summary>
    public class PerfectTrialInfo
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
        /// 難易度を取得します。
        /// </summary>
        public Difficulty Difficulty
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scoreLibraryId"></param>
        /// <param name="scoreHash"></param>
        /// <param name="difficulty"></param>
        public PerfectTrialInfo(int id, string scoreLibraryId, string scoreHash, Difficulty difficulty)
        {
            Id = id;
            ScoreLibraryId = scoreLibraryId;
            ScoreHash = scoreHash;
            Difficulty = difficulty;
        }
    }
}
