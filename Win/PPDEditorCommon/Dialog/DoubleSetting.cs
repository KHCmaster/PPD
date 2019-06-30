namespace PPDEditorCommon.Dialog
{
    public class DoubleSetting : NumberSetting<double>
    {
        public DoubleSetting(string name, string description, double defaultValue)
            : this(name, description, defaultValue, double.MinValue, double.MaxValue)
        {
        }

        public DoubleSetting(string name, string description, double defaultValue, double minValue, double MaxValue) :
            base(name, description, defaultValue, minValue, MaxValue)
        {
        }
    }
}
