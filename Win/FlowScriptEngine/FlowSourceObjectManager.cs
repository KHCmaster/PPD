using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public static class FlowSourceObjectManager
    {
        private static Dictionary<string, AssemblyAndType> dict;
        static FlowSourceObjectManager()
        {
            dict = new Dictionary<string, AssemblyAndType>();
        }

        internal static void AddFlowSourceObject(AssemblyAndType asmAndType)
        {
            if (!dict.ContainsKey(asmAndType.Type.FullName))
            {
                dict.Add(asmAndType.Type.FullName, asmAndType);
            }
        }

        public static FlowSourceObjectBase CreateSource(string fullName)
        {
            if (!dict.TryGetValue(fullName, out AssemblyAndType asmAndType))
            {
                throw new Exception(String.Format("There is no source named {0}", fullName));
            }
            return (FlowSourceObjectBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName);
        }
    }
}
