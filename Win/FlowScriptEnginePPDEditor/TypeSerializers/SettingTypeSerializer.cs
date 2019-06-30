using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPDEditor.TypeSerializers
{
    public class SettingTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var viewModel = (PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel)value;
            foreach (var p in viewModel.SettingPairs)
            {
                var pairElem = serializer.CreateElement("Pair");
                element.Add(pairElem);
                serializer.Serialize(pairElem, p);
            }
        }
    }
}
