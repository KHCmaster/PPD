using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class ExMarkDataSerializer : MarkDataTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(PPDFramework.PPDStructure.PPDData.ExMarkData); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            base.Serialize(serializer, element, value);
            var markData = (PPDFramework.PPDStructure.PPDData.ExMarkData)value;
            var releaseTimeElem = serializer.CreateElement("ReleaseTime");
            element.Add(releaseTimeElem);
            serializer.Serialize(releaseTimeElem, markData.EndTime);
        }
    }
}
