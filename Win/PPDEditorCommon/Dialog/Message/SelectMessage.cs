using GalaSoft.MvvmLight.Messaging;

namespace PPDEditorCommon.Dialog.Message
{
    class SelectMessage : MessageBase
    {
        public string ElementName
        {
            get;
            private set;
        }

        public SelectMessage(object sender, string elementName)
            : base(sender)
        {
            ElementName = elementName;
        }
    }
}
