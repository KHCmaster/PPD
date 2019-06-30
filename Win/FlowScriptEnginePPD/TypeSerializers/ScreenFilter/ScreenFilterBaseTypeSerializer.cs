using FlowScriptEngine;
using PPDFramework.ScreenFilters;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ScreenFilter
{
    public class ScreenFilterBaseTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(ScreenFilterBase); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (ScreenFilterBase)value;
        }
    }
}
