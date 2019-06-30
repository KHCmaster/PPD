namespace PPDFramework
{
    /// <summary>
    /// 入力情報を表すクラスです。
    /// </summary>
    public abstract class InputActionBase
    {
        /// <summary>
        /// ボタンの種類を取得します。
        /// </summary>
        public ButtonType ButtonType
        {
            get;
            private set;
        }

        /// <summary>
        /// 時間を取得します。
        /// </summary>
        public double Time
        {
            get;
            private set;
        }

        /// <summary>
        /// ボタンが押されたかどうかを取得します。
        /// </summary>
        public bool IsPressed
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <param name="time"></param>
        /// <param name="isPressed"></param>
        protected InputActionBase(ButtonType buttonType, double time, bool isPressed)
        {
            ButtonType = buttonType;
            Time = time;
            IsPressed = isPressed;
        }

        /// <summary>
        /// 入力のあった正確な時間を取得します。
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public abstract double GetAccurateTime(double currentTime);
    }
}