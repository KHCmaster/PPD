using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class HashSetTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(HashSet<object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var hashSet = (HashSet<object>)value;
            foreach (var obj in hashSet)
            {
                var newElem = serializer.CreateElement("");
                element.Add(newElem);
                serializer.Serialize(newElem, obj);
            }
        }
    }
}
