namespace PPDFramework
{
    /// <summary>
    /// 空の入力情報です。
    /// </summary>
    public class EmptyInputInfo : InputInfoBase
    {
        private static EmptyInputInfo instance = new EmptyInputInfo();

        /// <summary>
        /// シングルトンのインスタンスを取得します。
        /// </summary>
        public static EmptyInputInfo Instance
        {
            get
            {
                return instance;
            }
        }

        private EmptyInputInfo()
            : base(false)
        {
        }

        /// <summary>
        /// 押されているフレーム数を取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public override int GetPressingFrame(ButtonType buttonType)
        {
            return 0;
        }

        /// <summary>
        /// 押されたかどうかを取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public override bool IsPressed(ButtonType buttonType)
        {
            return false;
        }

        /// <summary>
        /// 離されたかどうかを取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public override bool IsReleased(ButtonType buttonType)
        {
            return false;
        }
    }
}
