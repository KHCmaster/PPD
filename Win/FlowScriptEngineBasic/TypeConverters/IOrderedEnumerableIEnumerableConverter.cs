using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class IOrderedEnumerableIEnumerableConverter : TemplateTypeConverter<IOrderedEnumerable<object>, IEnumerable<object>>
    {

        public override object Convert(object data)
        {
            return (IEnumerable<object>)data;
        }
    }
}