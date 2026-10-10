namespace PPDEditorCommon.Dialog
{
    public class BoolSetting(string name, string description, bool defaultValue) : SettingBase(name, description)
    {
        public bool DefaultValue
        {
            get;
            private set;
        } = defaultValue;
    }
}
