namespace PPDFramework
{
    /// <summary>
    /// 入力情報を扱うクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="accurate"></param>
    public abstract class InputInfoBase(bool accurate)
    {
        /// <summary>
        /// 正確な入力を扱うかどうかを取得します。
        /// </summary>
        public bool Accurate
        {
            get;
            private set;
        } = accurate;

        /// <summary>
        /// 押されたかどうかを取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public abstract bool IsPressed(ButtonType buttonType);

        /// <summary>
        /// 離されたかどうかを取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public abstract bool IsReleased(ButtonType buttonType);

        /// <summary>
        /// 押されているフレーム数を取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public abstract int GetPressingFrame(ButtonType buttonType);

        /// <summary>
        /// 入力イベントを取得します。
        /// </summary>
        public virtual InputActionBase[] Actions
        {
            get
            {
                return [];
            }
        }
    }
}
