namespace PPDEditorCommon.Dialog
{
    public abstract class SettingBase(string name, string description)
    {
        public string Name
        {
            get;
            private set;
        } = name;

        public string Description
        {
            get;
            private set;
        } = description;
    }
}
