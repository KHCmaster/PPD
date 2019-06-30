namespace PPDEditorCommon.Dialog
{
    public abstract class NumberSetting<T> : SettingBase where T : struct
    {
        public T DefaultValue
        {
            get;
            private set;
        }

        public T MinValue
        {
            get;
            private set;
        }

        public T MaxValue
        {
            get;
            private set;
        }

        protected NumberSetting(string name, string description, T defaultValue, T minValue, T maxValue) :
            base(name, description)
        {
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
