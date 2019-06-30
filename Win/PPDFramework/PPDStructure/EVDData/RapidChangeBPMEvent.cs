namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 急速BPM変更イベント
    /// </summary>
    public class RapidChangeBPMEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="bpm">BPM</param>
        /// <param name="rapid">急速変化かどうか</param>
        public RapidChangeBPMEvent(float time, float bpm, bool rapid)
            : base(time, EventType.RapidChangeBPM)
        {
            BPM = bpm;
            Rapid = rapid;
        }

        /// <summary>
        /// BPM
        /// </summary>
        public float BPM
        {
            get;
            private set;
        }

        /// <summary>
        /// 急速変化かどうか
        /// </summary>
        public bool Rapid
        {
            get;
            private set;
        }
    }
}
