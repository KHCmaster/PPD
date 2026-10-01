namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// スライドスケール変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="slideScale">スライドスケール</param>
    public class ChangeSlideScaleEvent(float time, float slideScale) : IEVDData(time, EventType.ChangeSlideScale)
    {

        /// <summary>
        /// スライドスケールを取得します
        /// </summary>
        public float SlideScale
        {
            get;
            private set;
        } = slideScale;
    }
}
