using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class PolygonObjectTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(PolygonObject); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var polygonObject = (PolygonObject)value;
            Serialize(serializer, element, polygonObject);
            var nameElem = serializer.CreateElement("Name");
            var primitiveTypeElem = serializer.CreateElement("PrimitiveType");
            var primitiveCountElem = serializer.CreateElement("PrimitiveCount");
            var startIndexElem = serializer.CreateElement("StartIndex");
            var vertexCountElem = serializer.CreateElement("VertexCount");
            element.Add(nameElem);
            element.Add(primitiveTypeElem);
            element.Add(primitiveCountElem);
            element.Add(startIndexElem);
            element.Add(vertexCountElem);
            serializer.Serialize(nameElem, polygonObject.ImageResource.FileName);
            serializer.Serialize(primitiveTypeElem, polygonObject.PrimitiveType);
            serializer.Serialize(primitiveCountElem, polygonObject.PrimitiveCount);
            serializer.Serialize(startIndexElem, polygonObject.StartIndex);
            serializer.Serialize(vertexCountElem, polygonObject.VertexCount);
        }
    }
}
