using System;
using System.Collections.Generic;
using System.Drawing;

namespace FlowScriptEngine
{
    public abstract class TypeColorBase
    {
        public abstract IEnumerable<KeyValuePair<Type, Color>> EnumerateColors();
    }
}
