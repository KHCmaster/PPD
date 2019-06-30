using FlowScriptEngine;
using SharpDX;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineSlimDX.TypeSerializers
{
    public class QuaternionTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Quaternion); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var quaternion = (Quaternion)value;
            var xElem = serializer.CreateElement("X");
            var yElem = serializer.CreateElement("Y");
            var zElem = serializer.CreateElement("Z");
            var wElem = serializer.CreateElement("W");
            var angleElem = serializer.CreateElement("Angle");
            var axisElem = serializer.CreateElement("Axis");
            element.Add(
                xElem,
                yElem,
                zElem,
                wElem,
                angleElem,
                axisElem);
            serializer.Serialize(xElem, quaternion.X);
            serializer.Serialize(yElem, quaternion.Y);
            serializer.Serialize(zElem, quaternion.Z);
            serializer.Serialize(wElem, quaternion.W);
            serializer.Serialize(angleElem, quaternion.Angle);
            serializer.Serialize(axisElem, quaternion.Axis);
        }
    }
}
