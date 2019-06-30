namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// BPM変更イベント
    /// </summary>
    public class ChangeBPMEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="bpm">BPM</param>
        public ChangeBPMEvent(float time, float bpm)
            : base(time, EventType.ChangeBPM)
        {
            BPM = bpm;
        }

        /// <summary>
        /// BPM
        /// </summary>
        public float BPM
        {
            get;
            private set;
        }
    }
}
