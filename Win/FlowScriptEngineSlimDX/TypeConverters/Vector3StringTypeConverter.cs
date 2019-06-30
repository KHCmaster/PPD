using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.TypeConverters
{
    public class Vector3StringTypeConverter : TemplateTypeConverter<SharpDX.Vector3, string>
    {
        public override object Convert(object data)
        {
            var vec3 = (SharpDX.Vector3)data;
            return vec3.ToString();
        }
    }
}
