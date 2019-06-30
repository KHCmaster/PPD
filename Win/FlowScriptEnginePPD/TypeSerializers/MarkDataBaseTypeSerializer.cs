using FlowScriptEngine;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class MarkDataBaseTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDFramework.PPDStructure.PPDData.MarkDataBase); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var markData = (PPDFramework.PPDStructure.PPDData.MarkDataBase)value;
            var rotationElem = serializer.CreateElement("Rotation");
            element.Add(rotationElem);
            serializer.Serialize(rotationElem, markData.Angle);
            var xElem = serializer.CreateElement("X");
            element.Add(xElem);
            serializer.Serialize(xElem, markData.X);
            var yElem = serializer.CreateElement("Y");
            element.Add(yElem);
            serializer.Serialize(yElem, markData.Y);
            var timeElem = serializer.CreateElement("Time");
            element.Add(timeElem);
            serializer.Serialize(timeElem, markData.Time);
            var idElem = serializer.CreateElement("ID");
            element.Add(idElem);
            serializer.Serialize(idElem, markData.ID);
            var markTypeElem = serializer.CreateElement("MarkType");
            element.Add(markTypeElem);
            serializer.Serialize(markTypeElem, markData.ButtonType);
            var parametersElem = serializer.CreateElement("Parameters");
            element.Add(parametersElem);
            serializer.Serialize(parametersElem, markData.Parameters.Cast<object>().ToArray());
        }
    }
}
