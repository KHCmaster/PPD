namespace PPDEditorCommon.Dialog.Message
{
    /// <summary>
    /// ビューモデルからビューへ送るメッセージの基底クラスです。
    /// </summary>
    abstract class MessageBase(object sender)
    {
        /// <summary>
        /// 送信者を取得します。
        /// </summary>
        public object Sender
        {
            get;
            private set;
        } = sender;
    }
}
