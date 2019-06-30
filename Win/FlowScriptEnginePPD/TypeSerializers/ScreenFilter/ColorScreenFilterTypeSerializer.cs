using FlowScriptEngine;
using PPDFramework.ScreenFilters;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ScreenFilter
{
    public class ColorScreenFilterTypeSerializer : ScreenFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(ColorScreenFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (ColorScreenFilter)value;
            AddNewElement(serializer, element, "Filters", filter.Filters.ToArray());
        }
    }
}
