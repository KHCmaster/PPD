using System;

namespace FlowScriptEngine
{
    public abstract class IToolTipText : Attribute
    {
        public abstract string Text { get; }
        public abstract string TextKey { get; }
        public abstract string Summary { get; }
    }
}
