using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class HashtableIEnumerableConverter : TemplateTypeConverter<Dictionary<object, object>, IEnumerable<object>>
    {

        public override object Convert(object data)
        {
            return ((Dictionary<object, object>)data).Cast<object>();
        }
    }
}
