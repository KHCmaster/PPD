using System.Collections.Generic;

namespace PPDEditorCommon.Dialog
{
    public class EnumerableSetting : SettingBase
    {
        public object DefaultValue
        {
            get;
            private set;
        }

        public IEnumerable<object> Enumerable
        {
            get;
            private set;
        }

        public EnumerableSetting(string name, string description, IEnumerable<object> enumerable, object defaultValue)
            : base(name, description)
        {
            Enumerable = enumerable;
            DefaultValue = defaultValue;
        }
    }
}
