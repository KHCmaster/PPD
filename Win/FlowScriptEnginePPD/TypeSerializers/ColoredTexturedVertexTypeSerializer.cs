using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class ColoredTexturedVertexTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(ColoredTexturedVertex); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var vertex = (ColoredTexturedVertex)value;
            var positionElem = serializer.CreateElement("Position");
            var textureCoordinatesElem = serializer.CreateElement("TextureCoordinates");
            var colorElem = serializer.CreateElement("Color");
            element.Add(positionElem);
            element.Add(textureCoordinatesElem);
            element.Add(colorElem);
            serializer.Serialize(positionElem, vertex.Position);
            serializer.Serialize(textureCoordinatesElem, vertex.TextureCoordinates);
            serializer.Serialize(colorElem, vertex.Color);
        }
    }
}
