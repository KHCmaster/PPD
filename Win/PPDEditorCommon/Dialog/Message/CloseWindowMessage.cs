using GalaSoft.MvvmLight.Messaging;

namespace PPDEditorCommon.Dialog.Message
{
    class CloseWindowMessage : MessageBase
    {
        public bool Result
        {
            get;
            private set;
        }

        public CloseWindowMessage(object sender, bool result)
            : base(sender)
        {
            Result = result;
        }
    }
}
