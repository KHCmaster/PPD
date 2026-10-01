namespace PPDFramework.Web
{
    /// <summary>
    /// 譜面情報の詳細
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="id">ID</param>
    /// <param name="startTime">開始時間</param>
    /// <param name="endTime">終了時間</param>
    public class WebSongInformationDetail(string id, float startTime, float endTime)
    {
        /// <summary>
        /// 開始時間
        /// </summary>
        public float StartTime
        {
            get;
            private set;
        } = startTime;

        /// <summary>
        /// 終了時間
        /// </summary>
        public float EndTime
        {
            get;
            private set;
        } = endTime;

        /// <summary>
        /// ID
        /// </summary>
        public string Id
        {
            get;
            private set;
        } = id;
    }
}
