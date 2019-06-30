namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// 同時押しの描画順序を変えるイベントクラスです
    /// </summary>
    public class ChangeInitializeOrderEvent : IEVDData
    {
        /// <summary>
        /// コンストラクタです
        /// </summary>
        /// <param name="time"></param>
        /// <param name="initializeOrder"></param>
        public ChangeInitializeOrderEvent(float time, ButtonType[] initializeOrder)
            : base(time, EventType.ChangeInitializeOrder)
        {
            InitializeOrder = initializeOrder;
        }

        /// <summary>
        /// 描画順序です
        /// </summary>
        public ButtonType[] InitializeOrder
        {
            get;
            private set;
        }
    }
}
