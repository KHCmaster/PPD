namespace PPDEditorCommon.Dialog
{
    public class IntSetting(string name, string description, int defaultValue, int minValue, int maxValue) : NumberSetting<int>(name, description, defaultValue, minValue, maxValue)
    {
        public IntSetting(string name, string description, int defaultValue) :
            this(name, description, defaultValue, int.MinValue, int.MaxValue)
        {
        }
    }
}
