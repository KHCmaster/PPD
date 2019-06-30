using FlowScriptEngine;
using System;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class DateTimeStringConverter : TemplateTypeConverter<DateTime, string>
    {

        public override object Convert(object data)
        {
            var dateTime = (DateTime)data;
            return dateTime.ToString(CultureInfo.InvariantCulture);
        }
    }
}
