namespace PPDEditorCommon.Dialog
{
    public abstract class SettingBase
    {
        public string Name
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        protected SettingBase(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
