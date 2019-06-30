using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class ModInfoTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDFramework.Mod.ModInfo); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var modInfo = (PPDFramework.Mod.ModInfo)value;
            var displayNameElem = serializer.CreateElement("DisplayName");
            element.Add(displayNameElem);
            serializer.Serialize(displayNameElem, modInfo.DisplayName);
            var authorNameElem = serializer.CreateElement("AuthorName");
            element.Add(authorNameElem);
            serializer.Serialize(authorNameElem, modInfo.AuthorName);
            var fileNameElem = serializer.CreateElement("FileName");
            element.Add(fileNameElem);
            serializer.Serialize(fileNameElem, modInfo.FileName);
        }
    }
}
