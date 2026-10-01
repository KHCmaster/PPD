namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 効果音再生方法変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="keepplaying">押している間か</param>
    /// <param name="channel">チャンネル</param>
    public class ChangeSoundPlayModeEvent(float time, bool keepplaying, int channel) : IEVDData(time, EventType.ChangeSoundPlayMode)
    {

        /// <summary>
        /// 押している間か
        /// </summary>
        public bool KeepPlaying
        {
            get;
            private set;
        } = keepplaying;

        /// <summary>
        /// チャンネル
        /// </summary>
        public int Channel
        {
            get;
            private set;
        } = channel;
    }
}
