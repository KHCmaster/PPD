using FlowScriptEngine;
using PPDFramework.ScreenFilters;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ScreenFilter
{
    public class GaussianFilterTypeSerializer : ScreenFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(GaussianFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (GaussianFilter)value;
            AddNewElement(serializer, element, "Disperson", filter.Disperson);
        }
    }
}
