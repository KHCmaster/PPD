using FlowScriptEngine;
using PPDSound;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class SoundResourceTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(SoundResource); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var soundResouce = (SoundResource)value;
            var nameElem = serializer.CreateElement("Name");
            element.Add(nameElem);
            serializer.Serialize(nameElem, soundResouce.Name);
        }
    }
}