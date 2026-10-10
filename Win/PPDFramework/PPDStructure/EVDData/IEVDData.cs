namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// イベントデータクラス
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="eventType">タイプ</param>
    public abstract class IEVDData(float time, EventType eventType)
    {

        /// <summary>
        /// 時間
        /// </summary>
        public float Time { get; private set; } = time;

        /// <summary>
        /// タイプ
        /// </summary>
        public EventType EventType { get; private set; } = eventType;
    }
}
