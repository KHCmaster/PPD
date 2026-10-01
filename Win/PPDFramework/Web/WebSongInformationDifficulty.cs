using PPDFrameworkCore;

namespace PPDFramework.Web
{
    /// <summary>
    /// ウェブ上の譜面の難易度の情報です。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="songInfo"></param>
    /// <param name="difficulty"></param>
    /// <param name="revision"></param>
    /// <param name="hash"></param>
    public class WebSongInformationDifficulty(WebSongInformation songInfo, Difficulty difficulty, int revision, string hash)
    {
        private byte[] hashAsBytes;

        /// <summary>
        /// WebSongInformation のインスタンスを取得します。
        /// </summary>
        public WebSongInformation WebSongInformation
        {
            get;
            private set;
        } = songInfo;

        /// <summary>
        /// 難易度を取得します。
        /// </summary>
        public Difficulty Difficulty
        {
            get;
            private set;
        } = difficulty;

        /// <summary>
        /// リビジョンを取得します。
        /// </summary>
        public int Revision
        {
            get;
            private set;
        } = revision;

        /// <summary>
        /// ハッシュを取得します。
        /// </summary>
        public string Hash
        {
            get;
            private set;
        } = hash;

        /// <summary>
        /// ハッシュのバイト表示を取得します。
        /// </summary>
        public byte[] HashAsBytes
        {
            get
            {
                hashAsBytes ??= CryptographyUtility.Parsex2String(Hash);
                return hashAsBytes;
            }
        }
    }
}
