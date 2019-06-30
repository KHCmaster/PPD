using System;

namespace FlowScriptEngine
{
    public abstract class TypeConverterFromBase
    {
        public abstract Type From { get; }

        public virtual bool CanConvert(Type to)
        {
            return false;
        }

        public abstract object Convert(object data, Type to);
    }
}
