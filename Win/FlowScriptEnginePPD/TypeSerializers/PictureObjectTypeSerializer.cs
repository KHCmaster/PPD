using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class PictureObjectTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(PictureObject); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var pictureObject = (PictureObject)value;
            Serialize(serializer, element, pictureObject);
            var nameElem = serializer.CreateElement("Name");
            element.Add(nameElem);
            serializer.Serialize(nameElem, pictureObject.ImageResource.FileName);
        }
    }
}
