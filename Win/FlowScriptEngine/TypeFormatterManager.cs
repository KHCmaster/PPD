using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public static class TypeFormatterManager
    {
        private static readonly Dictionary<Type, TypeFormatterBase> dict;
        static TypeFormatterManager()
        {
            dict = [];
        }

        public static TypeFormatterBase GetFormatter(Type type)
        {
            dict.TryGetValue(type, out TypeFormatterBase typeFormatter);
            return typeFormatter;
        }

        internal static void AddFormatter(TypeFormatterBase formatter)
        {
            if (!dict.TryAdd(formatter.Type, formatter))
            {
                Console.WriteLine(String.Format("Formatter {0} is multiple defined.", formatter));
            }
        }
    }
}
