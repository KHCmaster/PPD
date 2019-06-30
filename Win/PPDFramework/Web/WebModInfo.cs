using System.Collections.Generic;

namespace PPDFramework.Web
{
    /// <summary>
    /// Web上のModの情報を表すクラスです。
    /// </summary>
    public class WebModInfo
    {
        List<WebModInfoDetail> details;

        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// IDを取得します。
        /// </summary>
        public string Id
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
        /// 詳細の一覧を取得します。
        /// </summary>
        public WebModInfoDetail[] Details
        {
            get
            {
                return details.ToArray();
            }
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="name">名前。</param>
        /// <param name="id">ID。</param>
        /// <param name="revision">リビジョン。</param>
        public WebModInfo(string name, string id, int revision)
        {
            details = new List<WebModInfoDetail>();
            Name = name;
            Id = id;
            Revision = revision;
        }

        /// <summary>
        /// 詳細を追加します。
        /// </summary>
        /// <param name="detail"></param>
        public void AddDetail(WebModInfoDetail detail)
        {
            details.Add(detail);
        }
    }
}
