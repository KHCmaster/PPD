using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class DoubleStringConverter : TemplateTypeConverter<double, string>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToDouble(data);
            return val.ToString(CultureInfo.InvariantCulture);
        }
    }
}
