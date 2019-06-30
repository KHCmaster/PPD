using PPDFrameworkCore;

namespace PPDFramework.Web
{
    /// <summary>
    /// Web上のMod情報の詳細を表すクラスです。
    /// </summary>
    public class WebModInfoDetail
    {
        byte[] hashAsBytes;

        /// <summary>
        /// 親のMod情報を取得します。
        /// </summary>
        public WebModInfo WebModInfo
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
        /// ハッシュを文字列で取得します。
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
        /// リビジョンを取得します。
        /// </summary>
        public int Revision
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="webModInfo">親のModの情報。</param>
        /// <param name="hash">ハッシュ。</param>
        /// <param name="revision">リビジョン。</param>
        public WebModInfoDetail(WebModInfo webModInfo, string hash, int revision)
        {
            WebModInfo = webModInfo;
            Hash = hash;
            Revision = revision;
        }
    }
}
