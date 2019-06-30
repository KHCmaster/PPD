namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 長押し開放時サウンド再生変更イベント
    /// </summary>
    public class ChangeReleaseSoundEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="releasesound">離した時に再生するか</param>
        /// <param name="channel">チャンネル</param>
        public ChangeReleaseSoundEvent(float time, bool releasesound, int channel)
            : base(time, EventType.ChangeReleaseSound)
        {
            ReleaseSound = releasesound;
            Channel = channel;
        }

        /// <summary>
        /// 離した時に再生するか
        /// </summary>
        public bool ReleaseSound
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
