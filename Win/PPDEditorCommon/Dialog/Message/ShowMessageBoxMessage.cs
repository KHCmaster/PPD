
namespace PPDEditorCommon.Dialog.Message
{
    class ShowMessageBoxMessage(object sender, string text) : MessageBase(sender)
    {
        public string Text
        {
            get;
            private set;
        } = text;
    }
}
