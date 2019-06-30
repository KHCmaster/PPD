using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class NumberPictureObjectTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(NumberPictureObject); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var numberPictureObject = (NumberPictureObject)value;
            Serialize(serializer, element, numberPictureObject);
            var numberElem = serializer.CreateElement("Number");
            element.Add(numberElem);
            serializer.Serialize(numberElem, (int)numberPictureObject.Value);
        }
    }
}
