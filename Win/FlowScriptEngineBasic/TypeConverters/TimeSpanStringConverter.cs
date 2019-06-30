using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class TimeSpanStringConverter : TemplateTypeConverter<TimeSpan, string>
    {

        public override object Convert(object data)
        {
            var timeSpan = (TimeSpan)data;
            return timeSpan.ToString();
        }
    }
}
