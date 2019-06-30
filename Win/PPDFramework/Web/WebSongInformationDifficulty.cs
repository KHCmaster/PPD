using PPDFrameworkCore;

namespace PPDFramework.Web
{
    /// <summary>
    /// ウェブ上の譜面の難易度の情報です。
    /// </summary>
    public class WebSongInformationDifficulty
    {
        private byte[] hashAsBytes;

        /// <summary>
        /// WebSongInformation のインスタンスを取得します。
        /// </summary>
        public WebSongInformation WebSongInformation
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
        /// リビジョンを取得します。
        /// </summary>
        public int Revision
        {
            get;
            private set;
        }

        /// <summary>
        /// ハッシュを取得します。
        /// </summary>
        public string Hash
        {
            get;
            private set;
        }

        /// <summary>
        /// ハッシュのバイト表示を取得します。
        /// </summary>
        public byte[] HashAsBytes
        {
            get
            {
                if (hashAsBytes == null)
                {
                    hashAsBytes = CryptographyUtility.Parsex2String(Hash);
                }
                return hashAsBytes;
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="songInfo"></param>
        /// <param name="difficulty"></param>
        /// <param name="revision"></param>
        /// <param name="hash"></param>
        public WebSongInformationDifficulty(WebSongInformation songInfo, Difficulty difficulty, int revision, string hash)
        {
            WebSongInformation = songInfo;
            Difficulty = difficulty;
            Revision = revision;
            Hash = hash;
        }
    }
}
