using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class ArrayListTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(List<object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var list = (List<object>)value;
            for (int i = 0; i < list.Count; i++)
            {
                var newElem = serializer.CreateElement(String.Format("[{0}]", i));
                element.Add(newElem);
                serializer.Serialize(newElem, list[i]);
            }
        }
    }
}
