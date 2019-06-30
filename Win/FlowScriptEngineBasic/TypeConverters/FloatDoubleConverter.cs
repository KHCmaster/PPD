using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class FloatDoubleConverter : TemplateTypeConverter<float, double>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToDouble(data);
            return val;
        }
    }
}
