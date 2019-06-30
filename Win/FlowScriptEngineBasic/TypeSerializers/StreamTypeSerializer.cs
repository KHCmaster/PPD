using FlowScriptEngine;
using System;
using System.IO;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class StreamTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Stream); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var stream = (Stream)value;
            var lengthElem = serializer.CreateElement("Length");
            var positionElem = serializer.CreateElement("Position");
            element.Add(lengthElem, positionElem);
            serializer.Serialize(lengthElem, stream.Length);
            serializer.Serialize(positionElem, stream.Position);
        }
    }
}
