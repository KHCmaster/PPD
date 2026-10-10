namespace PPDEditorCommon.Dialog
{
    public class DoubleSetting(string name, string description, double defaultValue, double minValue, double MaxValue) : NumberSetting<double>(name, description, defaultValue, minValue, MaxValue)
    {
        public DoubleSetting(string name, string description, double defaultValue)
            : this(name, description, defaultValue, double.MinValue, double.MaxValue)
        {
        }
    }
}
