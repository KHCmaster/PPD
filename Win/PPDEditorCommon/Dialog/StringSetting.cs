namespace PPDEditorCommon.Dialog
{
    public class StringSetting : SettingBase
    {
        public string DefaultValue
        {
            get;
            private set;
        }

        public int MaxLength
        {
            get;
            private set;
        }

        public StringSetting(string name, string descrption, string defaultValue) :
            this(name, descrption, defaultValue, int.MaxValue)
        {
        }

        public StringSetting(string name, string descrption, string defaultValue, int maxLength) :
            base(name, descrption)
        {
            DefaultValue = defaultValue;
            MaxLength = maxLength;
        }
    }
}
