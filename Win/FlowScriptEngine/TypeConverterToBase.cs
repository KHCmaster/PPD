using System;

namespace FlowScriptEngine
{
    public abstract class TypeConverterToBase
    {
        public abstract Type To { get; }

        public virtual bool CanConvert(Type from)
        {
            return false;
        }

        public abstract object Convert(object data, Type from);
    }
}
