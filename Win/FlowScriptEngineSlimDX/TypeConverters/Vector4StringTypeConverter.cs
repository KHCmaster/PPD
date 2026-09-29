using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.TypeConverters
{
    public class Vector4StringTypeConverter : TemplateTypeConverter<SharpDX.Vector4, string>
    {
        public override object Convert(object data)
        {
            var vec4 = (SharpDX.Vector4)data;
            // SharpDX の Vector4.ToString と同じ書式で、数値は .NET Framework と同じ書式にする
            return string.Format(LegacyNumberFormat.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", vec4.X, vec4.Y, vec4.Z, vec4.W);
        }
    }
}
