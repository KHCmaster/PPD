namespace PPDMvvm
{
    /// <summary>
    /// メッセージの基底クラスです。
    /// </summary>
    public class MessageBase
    {
        /// <summary>
        /// 送信者を取得します。
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// 宛先を取得します。
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="sender">送信者。</param>
        public MessageBase(object sender)
        {
            Sender = sender;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="sender">送信者。</param>
        /// <param name="target">宛先。</param>
        public MessageBase(object sender, object target)
        {
            Sender = sender;
            Target = target;
        }
    }
}
