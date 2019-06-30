using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class HashSetArrayListConverter : TemplateTypeConverter<HashSet<object>, List<object>>
    {

        public override object Convert(object data)
        {
            return (IEnumerable<object>)data;
        }
    }
}
