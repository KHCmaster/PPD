using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class HashtableTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Dictionary<object, object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var dict = (Dictionary<object, object>)value;
            foreach (var p in dict)
            {
                var pairElem = serializer.CreateElement("Pair");
                element.Add(pairElem);
                serializer.Serialize(pairElem, p);
            }
        }
    }
}
