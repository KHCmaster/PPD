namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 効果音再生方法変更イベント
    /// </summary>
    public class ChangeSoundPlayModeEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="keepplaying">押している間か</param>
        /// <param name="channel">チャンネル</param>
        public ChangeSoundPlayModeEvent(float time, bool keepplaying, int channel)
            : base(time, EventType.ChangeSoundPlayMode)
        {
            KeepPlaying = keepplaying;
            Channel = channel;
        }

        /// <summary>
        /// 押している間か
        /// </summary>
        public bool KeepPlaying
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
