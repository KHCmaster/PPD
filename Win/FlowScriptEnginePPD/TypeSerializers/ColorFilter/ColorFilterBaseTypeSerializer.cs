using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class ColorFilterBaseTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(ColorFilterBase); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (ColorFilterBase)value;
            Serialize(serializer, element, (ColorFilterBase)value);
        }

        protected void Serialize(Serializer serializer, XElement element, ColorFilterBase filter)
        {
            AddNewElement(serializer, element, "Weight", filter.Weight);
        }
    }
}
