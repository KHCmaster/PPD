using System.Collections.Generic;

namespace PPDFramework.Web
{
    /// <summary>
    /// リストの情報を表すクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="title">名前です。</param>
    /// <param name="isWatch">ウォッチかどうかです。</param>
    public class ListInfo(string title, bool isWatch)
    {
        /// <summary>
        /// 譜面の情報一覧です。
        /// </summary>
        private readonly List<ListScoreInfo> scoreInfos = [];

        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Title
        {
            get;
            private set;
        } = title;

        /// <summary>
        /// ウォッチかどうかです。
        /// </summary>
        public bool IsWatch
        {
            get;
            private set;
        } = isWatch;

        /// <summary>
        /// 譜面の情報一覧を取得します。
        /// </summary>
        public ListScoreInfo[] Scores
        {
            get
            {
                return [.. scoreInfos];
            }
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
