using GalaSoft.MvvmLight.Messaging;

namespace PPDEditorCommon.Dialog.Message
{
    class OpenFileDialogMessage : MessageBase
    {
        public string Filter
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public bool Result
        {
            get;
            set;
        }

        public OpenFileDialogMessage(object sender, string filter)
            : base(sender)
        {
            Filter = filter;
        }
    }
}
