namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 急速BPM変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="bpm">BPM</param>
    /// <param name="rapid">急速変化かどうか</param>
    public class RapidChangeBPMEvent(float time, float bpm, bool rapid) : IEVDData(time, EventType.RapidChangeBPM)
    {

        /// <summary>
        /// BPM
        /// </summary>
        public float BPM
        {
            get;
            private set;
        } = bpm;

        /// <summary>
        /// 急速変化かどうか
        /// </summary>
        public bool Rapid
        {
            get;
            private set;
        } = rapid;
    }
}
