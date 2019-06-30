using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectStringConverter : TemplateTypeConverter<object, string>
    {

        public override object Convert(object data)
        {
            var val = data.ToString();
            return val;
        }
    }
}
