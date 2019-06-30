using System;

namespace FlowScriptEngine
{
    public abstract class IWarning : Attribute
    {
        public abstract string Text { get; }
        public abstract string TextKey { get; }
    }
}
