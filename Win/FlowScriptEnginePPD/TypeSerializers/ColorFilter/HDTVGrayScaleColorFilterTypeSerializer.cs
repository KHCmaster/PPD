using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class HDTVGrayScaleColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(HDTVGrayScaleColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (HDTVGrayScaleColorFilter)value;
            Serialize(serializer, element, filter);
            AddNewElement(serializer, element, "GammaCorrection", filter.GammaCorrection);
        }
    }
}
