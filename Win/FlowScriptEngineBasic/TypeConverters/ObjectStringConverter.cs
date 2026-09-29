using FlowScriptEngine;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectStringConverter : TemplateTypeConverter<object, string>
    {

        public override object Convert(object data)
        {
            // float / double は .NET Framework と同じ書式にする
            var val = LegacyNumberFormat.ToString(data);
            return val;
        }
    }
}
