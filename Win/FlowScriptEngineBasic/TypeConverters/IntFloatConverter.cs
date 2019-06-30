using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class IntFloatConverter : TemplateTypeConverter<int, float>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToSingle(data);
            return val;
        }
    }
}
