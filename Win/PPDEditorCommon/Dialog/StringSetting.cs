namespace PPDEditorCommon.Dialog
{
    public class StringSetting(string name, string descrption, string defaultValue, int maxLength) : SettingBase(name, descrption)
    {
        public string DefaultValue
        {
            get;
            private set;
        } = defaultValue;

        public int MaxLength
        {
            get;
            private set;
        } = maxLength;

        public StringSetting(string name, string descrption, string defaultValue) :
            this(name, descrption, defaultValue, int.MaxValue)
        {
        }
    }
}
