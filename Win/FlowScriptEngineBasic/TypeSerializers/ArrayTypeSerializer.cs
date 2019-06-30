using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class ArrayTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(object[]); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var array = (object[])value;
            for (int i = 0; i < array.Length; i++)
            {
                var newElem = serializer.CreateElement(String.Format("[{0}]", i));
                element.Add(newElem);
                serializer.Serialize(newElem, array[i]);
            }
        }
    }
}
