namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// イベントデータクラス
    /// </summary>
    public abstract class IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="eventType">タイプ</param>
        protected IEVDData(float time, EventType eventType)
        {
            Time = time;
            EventType = eventType;
        }

        /// <summary>
        /// 時間
        /// </summary>
        public float Time { get; private set; }

        /// <summary>
        /// タイプ
        /// </summary>
        public EventType EventType { get; private set; }
    }
}
