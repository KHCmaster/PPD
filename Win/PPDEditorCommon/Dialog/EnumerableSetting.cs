using System.Collections.Generic;

namespace PPDEditorCommon.Dialog
{
    public class EnumerableSetting(string name, string description, IEnumerable<object> enumerable, object defaultValue) : SettingBase(name, description)
    {
        public object DefaultValue
        {
            get;
            private set;
        } = defaultValue;

        public IEnumerable<object> Enumerable
        {
            get;
            private set;
        } = enumerable;
    }
}
