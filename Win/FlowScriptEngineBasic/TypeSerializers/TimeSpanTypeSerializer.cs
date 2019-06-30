using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class TimeSpanTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(TimeSpan); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var timeSpan = (TimeSpan)value;
            AddNewElement(serializer, element, "Days", timeSpan.Days);
            AddNewElement(serializer, element, "Hours", timeSpan.Hours);
            AddNewElement(serializer, element, "Minutes", timeSpan.Minutes);
            AddNewElement(serializer, element, "Milliseconds", timeSpan.Milliseconds);
        }
    }
}
