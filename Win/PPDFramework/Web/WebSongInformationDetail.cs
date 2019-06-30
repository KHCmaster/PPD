namespace PPDFramework.Web
{
    /// <summary>
    /// 譜面情報の詳細
    /// </summary>
    public class WebSongInformationDetail
    {
        /// <summary>
        /// 開始時間
        /// </summary>
        public float StartTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 終了時間
        /// </summary>
        public float EndTime
        {
            get;
            private set;
        }

        /// <summary>
        /// ID
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">終了時間</param>
        public WebSongInformationDetail(string id, float startTime, float endTime)
        {
            Id = id;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
