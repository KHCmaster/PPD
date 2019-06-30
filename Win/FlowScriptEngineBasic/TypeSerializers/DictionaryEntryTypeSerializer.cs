using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class DictionaryEntryTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(KeyValuePair<object, object>); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var pair = (KeyValuePair<object, object>)value;
            AddNewElement(serializer, element, "Key", pair.Key);
            AddNewElement(serializer, element, "Key", pair.Value);
        }
    }
}