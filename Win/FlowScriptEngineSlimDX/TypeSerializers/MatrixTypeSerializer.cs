using FlowScriptEngine;
using SharpDX;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineSlimDX.TypeSerializers
{
    public class MatrixTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Matrix); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var matrix = (Matrix)value;
            var m11Elem = serializer.CreateElement("M11");
            var m12Elem = serializer.CreateElement("M12");
            var m13Elem = serializer.CreateElement("M13");
            var m14Elem = serializer.CreateElement("M14");
            var m21Elem = serializer.CreateElement("M21");
            var m22Elem = serializer.CreateElement("M22");
            var m23Elem = serializer.CreateElement("M23");
            var m24Elem = serializer.CreateElement("M24");
            var m31Elem = serializer.CreateElement("M31");
            var m32Elem = serializer.CreateElement("M32");
            var m33Elem = serializer.CreateElement("M33");
            var m34Elem = serializer.CreateElement("M34");
            var m41Elem = serializer.CreateElement("M41");
            var m42Elem = serializer.CreateElement("M42");
            var m43Elem = serializer.CreateElement("M43");
            var m44Elem = serializer.CreateElement("M44");
            element.Add(
                m11Elem,
                m12Elem,
                m13Elem,
                m14Elem,
                m21Elem,
                m22Elem,
                m23Elem,
                m24Elem,
                m31Elem,
                m32Elem,
                m33Elem,
                m34Elem,
                m41Elem,
                m42Elem,
                m43Elem,
                m44Elem);
            serializer.Serialize(m11Elem, matrix.M11);
            serializer.Serialize(m12Elem, matrix.M12);
            serializer.Serialize(m13Elem, matrix.M13);
            serializer.Serialize(m14Elem, matrix.M14);
            serializer.Serialize(m21Elem, matrix.M21);
            serializer.Serialize(m22Elem, matrix.M22);
            serializer.Serialize(m23Elem, matrix.M23);
            serializer.Serialize(m24Elem, matrix.M24);
            serializer.Serialize(m31Elem, matrix.M31);
            serializer.Serialize(m32Elem, matrix.M32);
            serializer.Serialize(m33Elem, matrix.M33);
            serializer.Serialize(m34Elem, matrix.M34);
            serializer.Serialize(m41Elem, matrix.M41);
            serializer.Serialize(m42Elem, matrix.M42);
            serializer.Serialize(m43Elem, matrix.M43);
            serializer.Serialize(m44Elem, matrix.M44);
        }
    }
}
