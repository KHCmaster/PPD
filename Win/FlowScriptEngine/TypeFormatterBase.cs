using System;

namespace FlowScriptEngine
{
    public abstract class TypeFormatterBase
    {
        public abstract Type Type { get; }
        public abstract bool Format(string str, out object value);
        public virtual string[] AllowedPropertyString { get { return new string[0]; } }
        public virtual string CorrentFormat { get { return ""; } }
    }
}
