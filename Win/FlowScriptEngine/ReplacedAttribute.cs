using System;

namespace FlowScriptEngine
{
    public class ReplacedAttribute : Attribute
    {
        public string Name
        {
            get;
            private set;
        }

        public ReplacedAttribute(string name)
        {
            Name = name;
        }
    }
}
