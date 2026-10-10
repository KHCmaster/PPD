namespace PPDFramework.PPDStructure.PPDData
{
    /// <summary>
    /// 長押しマーククラス
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="angle">回転角</param>
    /// <param name="time">時間</param>
    /// <param name="endtime">終了時間</param>
    /// <param name="buttontype">ボタンタイプ</param>
    /// <param name="id">ID</param>
    public class ExMarkData(float x, float y, float angle, float time, float endtime, ButtonType buttontype, uint id) : MarkData(x, y, angle, time, buttontype, id)
    {

        /// <summary>
        /// 終了時間
        /// </summary>
        public float EndTime
        {
            get;
            private set;
        } = endtime;
    }
}
