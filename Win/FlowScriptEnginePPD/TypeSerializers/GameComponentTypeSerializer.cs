using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class GameComponentTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(GameComponent); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            Serialize(serializer, element, (GameComponent)value);
        }

        protected void Serialize(Serializer serializer, XElement element, GameComponent value)
        {
            var childrenCountElem = serializer.CreateElement("ChildrenCount");
            var alphaElem = serializer.CreateElement("Alpha");
            var hiddenElem = serializer.CreateElement("Hidden");
            var positionElem = serializer.CreateElement("Position");
            var rotationElem = serializer.CreateElement("Rotation");
            var rotationCenterElem = serializer.CreateElement("RotationCenter");
            var scaleElem = serializer.CreateElement("Scale");
            var scaleCenterElem = serializer.CreateElement("ScaleCenter");
            element.Add(
                childrenCountElem,
                alphaElem,
                hiddenElem,
                positionElem,
                rotationElem,
                rotationCenterElem,
                scaleElem,
                scaleCenterElem);
            serializer.Serialize(childrenCountElem, value.ChildrenCount);
            serializer.Serialize(alphaElem, value.Alpha);
            serializer.Serialize(hiddenElem, value.Hidden);
            serializer.Serialize(positionElem, value.Position);
            serializer.Serialize(rotationElem, value.Rotation);
            serializer.Serialize(rotationCenterElem, value.RotationCenter);
            serializer.Serialize(scaleElem, value.Scale);
            serializer.Serialize(scaleCenterElem, value.ScaleCenter);
        }
    }
}
