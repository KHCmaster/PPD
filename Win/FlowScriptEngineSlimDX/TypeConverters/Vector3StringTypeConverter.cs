using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.TypeConverters
{
    public class Vector3StringTypeConverter : TemplateTypeConverter<System.Numerics.Vector3, string>
    {
        public override object Convert(object data)
        {
            var vec3 = (System.Numerics.Vector3)data;
            // SharpDX の Vector3.ToString と同じ書式で、数値は .NET Framework と同じ書式にする
            return string.Format(LegacyNumberFormat.CurrentCulture, "X:{0} Y:{1} Z:{2}", vec3.X, vec3.Y, vec3.Z);
        }
    }
}
