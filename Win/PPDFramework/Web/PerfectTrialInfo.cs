using PPDFrameworkCore;

namespace PPDFramework.Web
{
    /// <summary>
    /// パーフェクトトライアルの情報を表すクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクターです。
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="scoreLibraryId"></param>
    /// <param name="scoreHash"></param>
    /// <param name="difficulty"></param>
    public class PerfectTrialInfo(int id, string scoreLibraryId, string scoreHash, Difficulty difficulty)
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
        /// 譜面IDを取得します。
        /// </summary>
        public string ScoreLibraryId
        {
            get;
            private set;
        } = scoreLibraryId;

        /// <summary>
        /// 譜面ハッシュを取得します。
        /// </summary>
        public string ScoreHash
        {
            get;
            private set;
        } = scoreHash;

        /// <summary>
        /// 難易度を取得します。
        /// </summary>
        public Difficulty Difficulty
        {
            get;
            private set;
        } = difficulty;
    }
}
