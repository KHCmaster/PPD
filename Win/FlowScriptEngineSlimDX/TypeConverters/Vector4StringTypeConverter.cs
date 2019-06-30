using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.TypeConverters
{
    public class Vector4StringTypeConverter : TemplateTypeConverter<SharpDX.Vector4, string>
    {
        public override object Convert(object data)
        {
            var vec4 = (SharpDX.Vector4)data;
            return vec4.ToString();
        }
    }
}
