using System;

namespace FlowScriptEngine
{
    public abstract class EnumTypeFormatterBase<T> : TypeFormatterBase where T : struct
    {
        private string[] allowed;
        private T[] values;
        protected EnumTypeFormatterBase()
        {
            values = (T[])Enum.GetValues(typeof(T));
            allowed = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                allowed[i] = values[i].ToString();
            }
        }

        public override Type Type
        {
            get { return typeof(T); }
        }

        public override bool Format(string str, out object value)
        {
            var index = Array.IndexOf(allowed, str);
            if (index < 0)
            {
                value = null;
                return false;
            }
            value = values[index];
            return true;
        }

        public override string[] AllowedPropertyString
        {
            get
            {
                return allowed;
            }
        }
    }
}
