using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public static class TypeFormatterManager
    {
        private static Dictionary<Type, TypeFormatterBase> dict;
        static TypeFormatterManager()
        {
            dict = new Dictionary<Type, TypeFormatterBase>();
        }

        public static TypeFormatterBase GetFormatter(Type type)
        {
            dict.TryGetValue(type, out TypeFormatterBase typeFormatter);
            return typeFormatter;
        }

        internal static void AddFormatter(TypeFormatterBase formatter)
        {
            if (!dict.ContainsKey(formatter.Type))
            {
                dict.Add(formatter.Type, formatter);
            }
            else
            {
                Console.WriteLine(String.Format("Formatter {0} is multiple defined.", formatter));
            }
        }
    }
}
