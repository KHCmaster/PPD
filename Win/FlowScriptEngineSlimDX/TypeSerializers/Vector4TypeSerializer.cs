using FlowScriptEngine;
using SharpDX;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineSlimDX.TypeSerializers
{
    public class Vector4TypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Vector4); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var vector = (Vector4)value;
            var xElem = serializer.CreateElement("X");
            var yElem = serializer.CreateElement("Y");
            var zElem = serializer.CreateElement("Z");
            var wElem = serializer.CreateElement("W");
            element.Add(xElem, yElem, zElem, wElem);
            serializer.Serialize(xElem, vector.X);
            serializer.Serialize(yElem, vector.Y);
            serializer.Serialize(zElem, vector.Z);
            serializer.Serialize(wElem, vector.W);
        }
    }
}
