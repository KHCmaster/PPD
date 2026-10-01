namespace PPDEditorCommon.Dialog
{
    public abstract class NumberSetting<T>(string name, string description, T defaultValue, T minValue, T maxValue) : SettingBase(name, description) where T : struct
    {
        public T DefaultValue
        {
            get;
            private set;
        } = defaultValue;

        public T MinValue
        {
            get;
            private set;
        } = minValue;

        public T MaxValue
        {
            get;
            private set;
        } = maxValue;
    }
}
