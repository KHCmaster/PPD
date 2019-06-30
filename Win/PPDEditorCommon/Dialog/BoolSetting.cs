namespace PPDEditorCommon.Dialog
{
    public class BoolSetting : SettingBase
    {
        public bool DefaultValue
        {
            get;
            private set;
        }

        public BoolSetting(string name, string description, bool defaultValue) :
            base(name, description)
        {
            DefaultValue = defaultValue;
        }
    }
}
