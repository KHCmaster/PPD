using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class IntSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(int); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            SetValue(element, value.ToString());
        }
    }
}
