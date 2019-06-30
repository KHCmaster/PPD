using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class FloatStringConverter : TemplateTypeConverter<float, string>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToSingle(data);
            return val.ToString(CultureInfo.InvariantCulture);
        }
    }
}
