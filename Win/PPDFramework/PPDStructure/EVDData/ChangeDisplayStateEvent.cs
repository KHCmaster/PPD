namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 表示タイプ
    /// </summary>
    public enum DisplayState
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal = 0,
        /// <summary>
        /// サドゥン
        /// </summary>
        Sudden = 1,
        /// <summary>
        /// ヒドゥン
        /// </summary>
        Hidden = 2,
        /// <summary>
        /// カラーのみサドゥン
        /// </summary>
        SuddenColor = 3,
        /// <summary>
        /// カラーのみヒドゥン
        /// </summary>
        HiddenColor = 4,
    }

    /// <summary>
    /// マーク表示タイプ変更イベントクラス
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="displaystate">表示タイプ</param>
    public class ChangeDisplayStateEvent(float time, DisplayState displaystate) : IEVDData(time, EventType.ChangeDisplayState)
    {
        /// <summary>
        /// 表示タイプ
        /// </summary>
        public DisplayState DisplayState
        {
            get;
            private set;
        } = displaystate;
    }
}
