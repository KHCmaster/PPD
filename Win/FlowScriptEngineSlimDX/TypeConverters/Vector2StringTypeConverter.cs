using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.TypeConverters
{
    public class Vector2StringTypeConverter : TemplateTypeConverter<SharpDX.Vector2, string>
    {
        public override object Convert(object data)
        {
            var vec2 = (SharpDX.Vector2)data;
            return vec2.ToString();
        }
    }
}
