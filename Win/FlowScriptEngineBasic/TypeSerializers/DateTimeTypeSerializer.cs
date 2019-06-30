using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class DateTimeTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(DateTime); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var dateTime = (DateTime)value;
            AddNewElement(serializer, element, "Year", dateTime.Year);
            AddNewElement(serializer, element, "Month", dateTime.Month);
            AddNewElement(serializer, element, "Day", dateTime.Day);
            AddNewElement(serializer, element, "Hour", dateTime.Hour);
            AddNewElement(serializer, element, "Minute", dateTime.Minute);
            AddNewElement(serializer, element, "Second", dateTime.Second);
            AddNewElement(serializer, element, "Millisecond", dateTime.Millisecond);
        }
    }
}
