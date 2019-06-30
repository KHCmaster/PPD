namespace PPDEditorCommon.Dialog
{
    public class IntSetting : NumberSetting<int>
    {
        public IntSetting(string name, string description, int defaultValue) :
            this(name, description, defaultValue, int.MinValue, int.MaxValue)
        {
        }

        public IntSetting(string name, string description, int defaultValue, int minValue, int maxValue) :
            base(name, description, defaultValue, minValue, maxValue)
        {
        }
    }
}
