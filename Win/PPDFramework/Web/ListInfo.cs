using System.Collections.Generic;

namespace PPDFramework.Web
{
    /// <summary>
    /// リストの情報を表すクラスです。
    /// </summary>
    public class ListInfo
    {
        /// <summary>
        /// 譜面の情報一覧です。
        /// </summary>
        private List<ListScoreInfo> scoreInfos;

        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// ウォッチかどうかです。
        /// </summary>
        public bool IsWatch
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面の情報一覧を取得します。
        /// </summary>
        public ListScoreInfo[] Scores
        {
            get
            {
                return scoreInfos.ToArray();
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="title">名前です。</param>
        /// <param name="isWatch">ウォッチかどうかです。</param>
        public ListInfo(string title, bool isWatch)
        {
            Title = title;
            IsWatch = isWatch;
            scoreInfos = new List<ListScoreInfo>();
        }

        /// <summary>
        /// リストの譜面情報を追加します。
        /// </summary>
        /// <param name="listScoreInfo">リストの譜面情報。</param>
        internal void Add(ListScoreInfo listScoreInfo)
        {
            scoreInfos.Add(listScoreInfo);
        }
    }
}
