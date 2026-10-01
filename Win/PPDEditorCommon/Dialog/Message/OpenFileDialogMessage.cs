
namespace PPDEditorCommon.Dialog.Message
{
    class OpenFileDialogMessage(object sender, string filter) : MessageBase(sender)
    {
        public string Filter
        {
            get;
            set;
        } = filter;

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
    }
}
