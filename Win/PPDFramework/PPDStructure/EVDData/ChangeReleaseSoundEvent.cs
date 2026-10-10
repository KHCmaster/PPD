namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 長押し開放時サウンド再生変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="releasesound">離した時に再生するか</param>
    /// <param name="channel">チャンネル</param>
    public class ChangeReleaseSoundEvent(float time, bool releasesound, int channel) : IEVDData(time, EventType.ChangeReleaseSound)
    {

        /// <summary>
        /// 離した時に再生するか
        /// </summary>
        public bool ReleaseSound
        {
            get;
            private set;
        } = releasesound;

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
