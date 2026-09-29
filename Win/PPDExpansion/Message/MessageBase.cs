namespace PPDExpansion.Message
{
    /// <summary>
    /// ビューモデルからビューへ送るメッセージの基底クラスです。
    /// </summary>
    abstract class MessageBase
    {
        /// <summary>
        /// 送信者を取得します。
        /// </summary>
        public object Sender
        {
            get;
            private set;
        }

        protected MessageBase(object sender)
        {
            Sender = sender;
        }
    }
}
