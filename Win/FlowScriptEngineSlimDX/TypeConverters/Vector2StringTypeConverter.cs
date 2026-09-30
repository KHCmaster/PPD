using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.TypeConverters
{
    public class Vector2StringTypeConverter : TemplateTypeConverter<System.Numerics.Vector2, string>
    {
        public override object Convert(object data)
        {
            var vec2 = (System.Numerics.Vector2)data;
            // SharpDX の Vector2.ToString と同じ書式で、数値は .NET Framework と同じ書式にする
            return string.Format(LegacyNumberFormat.CurrentCulture, "X:{0} Y:{1}", vec2.X, vec2.Y);
        }
    }
}
