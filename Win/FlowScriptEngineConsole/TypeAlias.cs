using FlowScriptEngine;
using System;
using System.Collections.Generic;

namespace FlowScriptEngineConsole
{
    public class TypeAlias : TypeAliasBase
    {
        Dictionary<Type, string> dictionary;
        public TypeAlias()
        {
            dictionary = new Dictionary<Type, string>
            {
                { typeof(Console), "Console" }
            };
        }

        public override IEnumerable<KeyValuePair<Type, string>> EnumerateAlias()
        {
            foreach (KeyValuePair<Type, string> kvp in dictionary)
            {
                yield return kvp;
            }
        }
    }
}
