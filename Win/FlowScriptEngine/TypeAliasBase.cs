using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public abstract class TypeAliasBase
    {
        public abstract IEnumerable<KeyValuePair<Type, string>> EnumerateAlias();
    }
}
