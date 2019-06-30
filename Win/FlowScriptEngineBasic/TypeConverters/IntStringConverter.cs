using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class IntStringConverter : TemplateTypeConverter<int, string>
    {

        public override object Convert(object data)
        {
            var num = System.Convert.ToInt32(data);
            return num.ToString();
        }
    }
}
