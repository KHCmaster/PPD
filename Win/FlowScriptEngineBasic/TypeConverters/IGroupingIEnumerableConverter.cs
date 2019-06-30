using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class IGroupingIEnumerableConverter : TemplateTypeConverter<IGrouping<object, object>, IEnumerable<object>>
    {

        public override object Convert(object data)
        {
            return (IEnumerable<object>)data;
        }
    }
}
