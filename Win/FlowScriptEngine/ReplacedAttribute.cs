using System;

namespace FlowScriptEngine
{
    public class ReplacedAttribute(string name) : Attribute
    {
        public string Name
        {
            get;
            private set;
        } = name;
    }
}
