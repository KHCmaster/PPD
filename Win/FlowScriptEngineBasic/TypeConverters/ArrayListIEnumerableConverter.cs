using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ArrayListIEnumerableConverter : TemplateTypeConverter<List<object>, IEnumerable<object>>
    {

        public override object Convert(object data)
        {
            return (IEnumerable<object>)data;
        }
    }
}
