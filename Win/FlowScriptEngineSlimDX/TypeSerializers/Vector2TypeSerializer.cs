using FlowScriptEngine;
using SharpDX;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineSlimDX.TypeSerializers
{
    public class Vector2TypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Vector2); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var vector = (Vector2)value;
            var xElem = serializer.CreateElement("X");
            var yElem = serializer.CreateElement("Y");
            element.Add(xElem, yElem);
            serializer.Serialize(xElem, vector.X);
            serializer.Serialize(yElem, vector.Y);
        }
    }
}
