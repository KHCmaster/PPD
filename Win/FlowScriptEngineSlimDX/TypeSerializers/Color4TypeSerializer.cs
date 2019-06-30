using FlowScriptEngine;
using SharpDX;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineSlimDX.TypeSerializers
{
    public class Color4TypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Color4); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var color = (Color4)value;
            var aElem = serializer.CreateElement("A");
            var rElem = serializer.CreateElement("R");
            var gElem = serializer.CreateElement("G");
            var bElem = serializer.CreateElement("B");
            element.Add(
                aElem,
                rElem,
                gElem,
                bElem);
            serializer.Serialize(aElem, color.Alpha);
            serializer.Serialize(rElem, color.Red);
            serializer.Serialize(gElem, color.Green);
            serializer.Serialize(bElem, color.Blue);
        }
    }
}
