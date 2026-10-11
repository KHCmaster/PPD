using System;

namespace FlowScriptEngine
{
    public abstract class EnumTypeFormatterBase<T> : TypeFormatterBase where T : struct
    {
        private readonly string[] allowed;
        private readonly T[] values;
        protected EnumTypeFormatterBase()
        {
            // 名前から作る（値から ToString すると、同じ値の別名（MessageBoxIcon の Hand と Stop など）が同じ名前になり、
            // 一覧に同じ名前が並び、別名で書いた値が読めなかった）
            allowed = Enum.GetNames(typeof(T));
            values = new T[allowed.Length];
            for (int i = 0; i < allowed.Length; i++)
            {
                values[i] = Enum.Parse<T>(allowed[i]);
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
