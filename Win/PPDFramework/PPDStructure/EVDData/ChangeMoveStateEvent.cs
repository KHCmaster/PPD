namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 移動タイプ
    /// </summary>
    public enum MoveState
    {
        /// <summary>
        /// なし
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 停止
        /// </summary>
        Stop = 1
    }

    /// <summary>
    /// 移動タイプ変更イベント
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="movestate">移動タイプ</param>
    public class ChangeMoveStateEvent(float time, MoveState movestate) : IEVDData(time, EventType.ChangeMoveState)
    {

        /// <summary>
        /// 移動タイプ
        /// </summary>
        public MoveState MoveState
        {
            get;
            private set;
        } = movestate;
    }
}
