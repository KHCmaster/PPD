using FlowScriptEngine;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class ILookupTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(ILookup<object, object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var lookup = (ILookup<object, object>)value;
            var countElem = serializer.CreateElement("Count");
            element.Add(countElem);
            serializer.Serialize(countElem, lookup.Count);
            int i = 0;
            foreach (var obj in lookup)
            {
                var newElem = serializer.CreateElement(String.Format("[{0}]", i));
                element.Add(newElem);
                serializer.Serialize(newElem, obj);
                i++;
            }
        }
    }
}
