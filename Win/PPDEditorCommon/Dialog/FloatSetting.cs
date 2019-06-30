namespace PPDEditorCommon.Dialog
{
    public class FloatSetting : NumberSetting<float>
    {
        public FloatSetting(string name, string description, float defaultValue)
            : this(name, description, defaultValue, float.MinValue, float.MaxValue)
        {
        }

        public FloatSetting(string name, string description, float defaultValue, float minValue, float MaxValue) :
            base(name, description, defaultValue, minValue, MaxValue)
        {
        }
    }
}
