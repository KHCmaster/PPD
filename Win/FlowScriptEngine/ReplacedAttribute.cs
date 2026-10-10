using System;

namespace FlowScriptEngine
{
    [AttributeUsage(AttributeTargets.All)]
    public class ReplacedAttribute(string name) : Attribute
    {
        public string Name
        {
            get;
            private set;
        } = name;
    }
}
