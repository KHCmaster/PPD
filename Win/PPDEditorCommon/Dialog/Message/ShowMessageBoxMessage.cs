using GalaSoft.MvvmLight.Messaging;

namespace PPDEditorCommon.Dialog.Message
{
    class ShowMessageBoxMessage : MessageBase
    {
        public string Text
        {
            get;
            private set;
        }

        public ShowMessageBoxMessage(object sender, string text)
            : base(sender)
        {
            Text = text;
        }
    }
}
