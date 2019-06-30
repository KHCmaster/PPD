using FlowScriptEngine;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class IGroupingTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(IGrouping<object, object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var grouping = (IGrouping<object, object>)value;
            int i = 0;
            var keyElem = serializer.CreateElement("Key");
            element.Add(keyElem);
            serializer.Serialize(keyElem, grouping.Key);
            foreach (var obj in grouping)
            {
                var newElem = serializer.CreateElement(String.Format("[{0}]", i));
                element.Add(newElem);
                serializer.Serialize(newElem, obj);
                i++;
            }
        }
    }
}
