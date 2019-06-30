using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class MarkDataTypeSerializer : MarkDataBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(PPDFramework.PPDStructure.PPDData.MarkData); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            base.Serialize(serializer, element, value);
        }
    }
}
