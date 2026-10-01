namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// ボリューム変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="volume">ボリューム</param>
    /// <param name="channel">チャンネル</param>
    public class ChangeVolumeEvent(float time, int volume, int channel) : IEVDData(time, EventType.ChangeVolume)
    {

        /// <summary>
        /// ボリューム
        /// </summary>
        public int Volume
        {
            get;
            private set;
        } = volume;

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
