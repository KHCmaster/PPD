using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class IntDoubleConverter : TemplateTypeConverter<int, double>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToDouble(data);
            return val;
        }
    }
}
