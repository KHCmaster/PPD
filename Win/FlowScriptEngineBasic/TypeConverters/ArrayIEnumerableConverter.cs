using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ArrayIEnumerableConverter : TemplateTypeConverter<object[], IEnumerable<object>>
    {

        public override object Convert(object data)
        {
            return (object[])data;
        }
    }
}
