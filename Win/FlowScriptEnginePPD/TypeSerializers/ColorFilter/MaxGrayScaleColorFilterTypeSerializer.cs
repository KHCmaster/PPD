using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class MaxGrayScaleColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(MaxGrayScaleColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (MaxGrayScaleColorFilter)value;
            Serialize(serializer, element, filter);
        }
    }
}
