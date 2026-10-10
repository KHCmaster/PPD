namespace PPDEditorCommon.Dialog
{
    public class FloatSetting(string name, string description, float defaultValue, float minValue, float MaxValue) : NumberSetting<float>(name, description, defaultValue, minValue, MaxValue)
    {
        public FloatSetting(string name, string description, float defaultValue)
            : this(name, description, defaultValue, float.MinValue, float.MaxValue)
        {
        }
    }
}
