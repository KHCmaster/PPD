using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectBooleanConverter : TemplateTypeConverter<object, bool>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToBoolean(data);
            return val;
        }
    }
}
