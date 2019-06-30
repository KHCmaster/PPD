using FlowScriptEngine;
using PPDFramework.Vertex;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class VertexInfoSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(VertexInfo); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var vertexInfo = (VertexInfo)value;
            var positionElem = serializer.CreateElement("Position");
            var textureCoordinatesElem = serializer.CreateElement("TextureCoordinates");
            var countElem = serializer.CreateElement("Count");
            element.Add(countElem);
            serializer.Serialize(countElem, vertexInfo.Count);
        }
    }
}
