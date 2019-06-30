using FlowScriptEngine;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FlowScriptEnginePPDEditor.TypeSerializers
{
    public class ParameterPresetTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDEditorCommon.ParameterPreset); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var parameterPreset = (PPDEditorCommon.ParameterPreset)value;
            AddNewElement(serializer, element, "PresetName", parameterPreset.PresetName);
            AddNewElement(serializer, element, "Parameters", parameterPreset.Parameters.Cast<object>().ToArray());
        }
    }
}
