using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class ColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(PPDFramework.Shaders.ColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (PPDFramework.Shaders.ColorFilter)value;
            Serialize(serializer, element, filter);
            AddNewElement(serializer, element, "Color", filter.Color);
        }
    }
}
