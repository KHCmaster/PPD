using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FlowScriptEnginePPDEditor
{
    public class TypeColors : TypeColorBase
    {
        Dictionary<Type, Color> dictionary;
        public TypeColors()
        {
            dictionary = new Dictionary<Type, Color>
            {
                { typeof(System.Windows.Forms.MessageBoxButtons), Color.FromArgb(0, 70, 102) },
                { typeof(System.Windows.Forms.MessageBoxIcon), Color.FromArgb(0, 70, 102) },
                { typeof(System.Windows.Forms.DialogResult), Color.FromArgb(0, 70, 102) },
                { typeof(PPDEditorCommon.ILayer), Color.FromArgb(0, 211, 219) },
                { typeof(PPDEditorCommon.IEditorMarkInfo), Color.FromArgb(157, 251, 254) },
                { typeof(PPDEditorCommon.IPosAndAngle), Color.FromArgb(207, 251, 254) },
                { typeof(PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel), Color.FromArgb(186, 219, 0) },
                { typeof(PPDEditorCommon.EventData), Color.FromArgb(178, 189, 116) },
                { typeof(PPDEditorCommon.ParameterPreset), Color.FromArgb(110, 189, 116) },
                { typeof(PPDFramework.PPDStructure.EVDData.DisplayState), Color.FromArgb(0, 70, 102) },
                { typeof(PPDFramework.PPDStructure.EVDData.MoveState), Color.FromArgb(0, 70, 102) },
                { typeof(PPDFramework.NoteType), Color.FromArgb(0, 70, 102) }
            };
        }

        public override IEnumerable<KeyValuePair<Type, Color>> EnumerateColors()
        {
            return dictionary;
        }
    }
}
