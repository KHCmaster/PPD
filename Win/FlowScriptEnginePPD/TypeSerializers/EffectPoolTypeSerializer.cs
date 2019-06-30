using FlowScriptEngine;
using PPDCoreModel;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class EffectPoolTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(EffectPool); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var pool = (EffectPool)value;
            var pathElem = serializer.CreateElement("Path");
            element.Add(pathElem);
            serializer.Serialize(pathElem, pool.FileName);
            var countElem = serializer.CreateElement("Count");
            element.Add(countElem);
            serializer.Serialize(countElem, pool.Count);
        }
    }
}
