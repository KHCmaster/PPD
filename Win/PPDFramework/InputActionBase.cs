namespace PPDFramework
{
    /// <summary>
    /// 入力情報を表すクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="buttonType"></param>
    /// <param name="time"></param>
    /// <param name="isPressed"></param>
    public abstract class InputActionBase(ButtonType buttonType, double time, bool isPressed)
    {
        /// <summary>
        /// ボタンの種類を取得します。
        /// </summary>
        public ButtonType ButtonType
        {
            get;
            private set;
        } = buttonType;

        /// <summary>
        /// 時間を取得します。
        /// </summary>
        public double Time
        {
            get;
            private set;
        } = time;

        /// <summary>
        /// ボタンが押されたかどうかを取得します。
        /// </summary>
        public bool IsPressed
        {
            get;
            private set;
        } = isPressed;

        /// <summary>
        /// 入力のあった正確な時間を取得します。
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public abstract double GetAccurateTime(double currentTime);
    }
}