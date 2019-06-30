using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class TextureStringTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(TextureString); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var textureString = (TextureString)value;
            Serialize(serializer, element, textureString);
            var textElem = serializer.CreateElement("Text");
            element.Add(textElem);
            serializer.Serialize(textElem, textureString.Text);
        }
    }
}
