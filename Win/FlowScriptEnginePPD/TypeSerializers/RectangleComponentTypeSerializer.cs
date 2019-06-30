using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class RectangleComponentTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(RectangleComponent); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var rectangleComponent = (RectangleComponent)value;
            Serialize(serializer, element, rectangleComponent);
            var rectangleWidthElem = serializer.CreateElement("RectangleWidth");
            var rectangleHeightElem = serializer.CreateElement("RectangleHeight");
            element.Add(rectangleWidthElem, rectangleHeightElem);
            serializer.Serialize(rectangleWidthElem, rectangleComponent.RectangleWidth);
            serializer.Serialize(rectangleHeightElem, rectangleComponent.RectangleHeight);
        }
    }
}
