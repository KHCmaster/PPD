using FlowScriptEngine;
using System;
using System.Collections.Generic;

namespace FlowScriptEnginePPDEditor
{
    public class TypeAlias : TypeAliasBase
    {
        Dictionary<Type, string> dictionary;
        public TypeAlias()
        {
            dictionary = new Dictionary<Type, string>
            {
                { typeof(System.Windows.Forms.MessageBoxButtons), "PPDEditor.MessageBox.ButtonType" },
                { typeof(System.Windows.Forms.MessageBoxIcon), "PPDEditor.MessageBox.IconType" },
                { typeof(System.Windows.Forms.DialogResult), "PPDEditor.MessageBox.ResultType" },
                { typeof(PPDEditorCommon.ILayer), "PPDEditor.Layer" },
                { typeof(PPDEditorCommon.IEditorMarkInfo), "PPDEditor.Mark" },
                { typeof(PPDEditorCommon.IPosAndAngle), "PPDEditor.Mark.PosAndRotation" },
                { typeof(PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel), "PPDEditor.Dialog.Setting" },
                { typeof(PPDEditorCommon.EventData), "PPDEditor.Event" },
                { typeof(PPDEditorCommon.ParameterPreset), "PPDEditor.Mark.ParameterPreset" },
                { typeof(PPDFramework.PPDStructure.EVDData.DisplayState), "PPDEditor.Event.DisplayState" },
                { typeof(PPDFramework.PPDStructure.EVDData.MoveState), "PPDEditor.Event.MoveState" },
                { typeof(PPDFramework.NoteType), "PPDEditor.Event.NoteType" }
            };
        }

        public override IEnumerable<KeyValuePair<Type, string>> EnumerateAlias()
        {
            return dictionary;
        }
    }
}
