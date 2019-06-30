namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// スライドスケール変更イベント
    /// </summary>
    public class ChangeSlideScaleEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="slideScale">スライドスケール</param>
        public ChangeSlideScaleEvent(float time, float slideScale)
            : base(time, EventType.ChangeSlideScale)
        {
            SlideScale = slideScale;
        }

        /// <summary>
        /// スライドスケールを取得します
        /// </summary>
        public float SlideScale
        {
            get;
            private set;
        }
    }
}
