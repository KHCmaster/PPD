namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 同時押しの描画順序を変えるイベントクラスです
    /// </summary>
    /// <remarks>
    /// コンストラクタです
    /// </remarks>
    /// <param name="time"></param>
    /// <param name="initializeOrder"></param>
    public class ChangeInitializeOrderEvent(float time, ButtonType[] initializeOrder) : IEVDData(time, EventType.ChangeInitializeOrder)
    {

        /// <summary>
        /// 描画順序です
        /// </summary>
        public ButtonType[] InitializeOrder
        {
            get;
            private set;
        } = initializeOrder;
    }
}
