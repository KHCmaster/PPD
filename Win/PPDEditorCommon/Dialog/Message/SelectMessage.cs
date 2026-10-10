
namespace PPDEditorCommon.Dialog.Message
{
    class SelectMessage(object sender, string elementName) : MessageBase(sender)
    {
        public string ElementName
        {
            get;
            private set;
        } = elementName;
    }
}
