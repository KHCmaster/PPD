using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class IEnumerableTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(IEnumerable<object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var enumerable = (IEnumerable<object>)value;
            int i = 0;
            foreach (var obj in enumerable)
            {
                var newElem = serializer.CreateElement(String.Format("[{0}]", i));
                element.Add(newElem);
                serializer.Serialize(newElem, obj);
                i++;
            }
        }
    }
}
