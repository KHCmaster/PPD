namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// ボリューム変更イベント
    /// </summary>
    public class ChangeVolumeEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="volume">ボリューム</param>
        /// <param name="channel">チャンネル</param>
        public ChangeVolumeEvent(float time, int volume, int channel)
            : base(time, EventType.ChangeVolume)
        {
            Volume = volume;
            Channel = channel;
        }

        /// <summary>
        /// ボリューム
        /// </summary>
        public int Volume
        {
            get;
            private set;
        }

        /// <summary>
        /// チャンネル
        /// </summary>
        public int Channel
        {
            get;
            private set;
        }
    }
}
