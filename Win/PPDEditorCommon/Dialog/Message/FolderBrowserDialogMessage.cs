using GalaSoft.MvvmLight.Messaging;

namespace PPDEditorCommon.Dialog.Message
{
    class FolderBrowserDialogMessage : MessageBase
    {
        public string SelectedPath
        {
            get;
            set;
        }

        public bool Result
        {
            get;
            set;
        }

        public FolderBrowserDialogMessage(object sender)
            : base(sender)
        {
        }
    }
}
