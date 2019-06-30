using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class AverageGrayScaleColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(AverageGrayScaleColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (AverageGrayScaleColorFilter)value;
            Serialize(serializer, element, filter);
        }
    }
}
