using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class BrightnessColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(BrightnessColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (BrightnessColorFilter)value;
            Serialize(serializer, element, filter);
            AddNewElement(serializer, element, "Scale", filter.Scale);
        }
    }
}
