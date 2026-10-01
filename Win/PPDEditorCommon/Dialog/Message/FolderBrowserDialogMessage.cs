
namespace PPDEditorCommon.Dialog.Message
{
    class FolderBrowserDialogMessage(object sender) : MessageBase(sender)
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
    }
}
