using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class DoubleStringConverter : TemplateTypeConverter<double, string>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToDouble(data);
            // .NET Framework と同じ書式（有効数字 15 桁）にする
            return LegacyNumberFormat.ToString(val, CultureInfo.InvariantCulture);
        }
    }
}
