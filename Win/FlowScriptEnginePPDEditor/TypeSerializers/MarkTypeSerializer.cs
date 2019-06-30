using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPDEditor.TypeSerializers
{
    public class MarkTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDEditorCommon.IEditorMarkInfo); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var mark = (PPDEditorCommon.IEditorMarkInfo)value;
            AddNewElement(serializer, element, "Position", mark.Position);
            AddNewElement(serializer, element, "Rotation", mark.Angle);
            AddNewElement(serializer, element, "Time", mark.Time);
            AddNewElement(serializer, element, "ID", (int)mark.ID);
            AddNewElement(serializer, element, "Type", mark.Type);
            AddNewElement(serializer, element, "IsLong", mark.IsLong);
            AddNewElement(serializer, element, "ReleaseTime", mark.ReleaseTime);
            AddNewElement(serializer, element, "Parameters", mark.Parameters);
        }
    }
}
