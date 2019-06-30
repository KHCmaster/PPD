using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class BooleanStringConverter : TemplateTypeConverter<bool, string>
    {

        public override object Convert(object data)
        {
            var val = System.Convert.ToBoolean(data);
            return val.ToString();
        }
    }
}
