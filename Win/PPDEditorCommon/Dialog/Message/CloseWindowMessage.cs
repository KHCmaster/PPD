
namespace PPDEditorCommon.Dialog.Message
{
    class CloseWindowMessage(object sender, bool result) : MessageBase(sender)
    {
        public bool Result
        {
            get;
            private set;
        } = result;
    }
}
