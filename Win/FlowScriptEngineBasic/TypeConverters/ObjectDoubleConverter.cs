using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectDoubleConverter : TemplateTypeConverter<object, double>
    {
        public override object Convert(object data)
        {
            var val = System.Convert.ToDouble(data);
            return val;
        }
    }
}
