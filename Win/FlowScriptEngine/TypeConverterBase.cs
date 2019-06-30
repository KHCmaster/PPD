using System;

namespace FlowScriptEngine
{
    public abstract class TypeConverterBase
    {
        public abstract Type From { get; }
        public abstract Type To { get; }
        public abstract object Convert(object data);
    }
}
