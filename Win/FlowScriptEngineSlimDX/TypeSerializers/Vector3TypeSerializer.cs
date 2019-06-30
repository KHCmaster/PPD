using FlowScriptEngine;
using SharpDX;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineSlimDX.TypeSerializers
{
    public class Vector3TypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Vector3); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var vector = (Vector3)value;
            var xElem = serializer.CreateElement("X");
            var yElem = serializer.CreateElement("Y");
            var zElem = serializer.CreateElement("Z");
            element.Add(xElem, yElem, zElem);
            serializer.Serialize(xElem, vector.X);
            serializer.Serialize(yElem, vector.Y);
            serializer.Serialize(zElem, vector.Z);
        }
    }
}
