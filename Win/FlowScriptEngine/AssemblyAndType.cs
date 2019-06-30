using System;
using System.Reflection;

namespace FlowScriptEngine
{
    public class AssemblyAndType
    {
        internal AssemblyAndType(Assembly asm, Type type)
        {
            Assembly = asm;
            Type = type;
        }

        public Type Type
        {
            get;
            private set;
        }

        public Assembly Assembly
        {
            get;
            private set;
        }

        public string FullName
        {
            get
            {
                return Type.FullName;
            }
        }
    }
}
