using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPDEditor.TypeSerializers
{
    public class LayerTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDEditorCommon.ILayer); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var layer = (PPDEditorCommon.ILayer)value;
            AddNewElement(serializer, element, "SelectedMarks", layer.SelectedMarks);
            AddNewElement(serializer, element, "Marks", layer.Marks);
            AddNewElement(serializer, element, "SelectedMark", layer.SelectedMark);
            AddNewElement(serializer, element, "IsSelected", layer.IsSelected);
        }
    }
}
