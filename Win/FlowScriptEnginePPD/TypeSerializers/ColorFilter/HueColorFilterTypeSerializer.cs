using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class HueColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(HueColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (HueColorFilter)value;
            Serialize(serializer, element, filter);
            AddNewElement(serializer, element, "Rotation", filter.Rotation);
        }
    }
}
