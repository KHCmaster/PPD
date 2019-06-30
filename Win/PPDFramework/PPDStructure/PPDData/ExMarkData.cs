namespace PPDFramework.PPDStructure.PPDData
{
    /// <summary>
    /// 長押しマーククラス
    /// </summary>
    public class ExMarkData : MarkData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="angle">回転角</param>
        /// <param name="time">時間</param>
        /// <param name="endtime">終了時間</param>
        /// <param name="buttontype">ボタンタイプ</param>
        /// <param name="id">ID</param>
        public ExMarkData(float x, float y, float angle, float time, float endtime, ButtonType buttontype, uint id)
            : base(x, y, angle, time, buttontype, id)
        {
            EndTime = endtime;
        }

        /// <summary>
        /// 終了時間
        /// </summary>
        public float EndTime
        {
            get;
            private set;
        }
    }
}
