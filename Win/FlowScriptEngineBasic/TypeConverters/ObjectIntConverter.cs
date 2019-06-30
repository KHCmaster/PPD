using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectIntConverter : TemplateTypeConverter<object, int>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToInt32(data);
            return val;
        }
    }
}
