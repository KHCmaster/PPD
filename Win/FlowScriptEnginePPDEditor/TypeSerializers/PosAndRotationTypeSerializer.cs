using FlowScriptEngine;
using PPDEditorCommon;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPDEditor.TypeSerializers
{
    public class PosAndRotationTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDEditorCommon.IPosAndAngle); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var posAndAngle = (IPosAndAngle)value;
            AddNewElement(serializer, element, "Position", posAndAngle.Position.Value);
            AddNewElement(serializer, element, "Rotation", posAndAngle.Rotation.Value);
            AddNewElement(serializer, element, "HasPosition", posAndAngle.Position.HasValue);
            AddNewElement(serializer, element, "HasRotation", posAndAngle.Rotation.HasValue);
        }
    }
}
