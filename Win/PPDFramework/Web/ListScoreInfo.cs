namespace PPDFramework.Web
{
    /// <summary>
    /// リストの譜面の情報を扱うクラスです。
    /// </summary>
    public class ListScoreInfo
    {
        /// <summary>
        /// 譜面IDです。
        /// </summary>
        public string ScoreId
        {
            get;
            private set;
        }

        /// <summary>
        /// タイトルです。
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// 親です。
        /// </summary>
        public ListInfo Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// リストの譜面の情報です。
        /// </summary>
        /// <param name="parent">親です。</param>
        /// <param name="scoreId">譜面ID。</param>
        /// <param name="title">タイトル。</param>
        internal ListScoreInfo(ListInfo parent, string scoreId, string title)
        {
            Parent = parent;
            ScoreId = scoreId;
            Title = title;
        }
    }
}
