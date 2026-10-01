namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// BPM変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="bpm">BPM</param>
    public class ChangeBPMEvent(float time, float bpm) : IEVDData(time, EventType.ChangeBPM)
    {

        /// <summary>
        /// BPM
        /// </summary>
        public float BPM
        {
            get;
            private set;
        } = bpm;
    }
}
