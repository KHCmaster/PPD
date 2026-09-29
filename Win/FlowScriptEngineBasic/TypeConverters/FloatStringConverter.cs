using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class FloatStringConverter : TemplateTypeConverter<float, string>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToSingle(data);
            // .NET Framework と同じ書式（有効数字 7 桁）にする
            return LegacyNumberFormat.ToString(val, CultureInfo.InvariantCulture);
        }
    }
}
