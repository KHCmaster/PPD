using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectFloatConverter : TemplateTypeConverter<object, float>
    {
        public override object Convert(object data)
        {
            var val = System.Convert.ToSingle(data);
            return val;
        }
    }
}
