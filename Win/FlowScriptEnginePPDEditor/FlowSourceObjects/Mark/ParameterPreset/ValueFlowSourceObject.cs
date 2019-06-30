using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark.ParameterPreset
{
    [ToolTipText("Mark_ParameterPreset_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.ParameterPreset.Value"; }
        }

        [ToolTipText("Mark_ParameterPreset_Value_Value")]
        public PPDEditorCommon.ParameterPreset Value
        {
            private get;
            set;
        }

        [ToolTipText("Mark_ParameterPreset_Value_PresetName")]
        public string PresetName
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.PresetName;
                }
                return "";
            }
        }

        [ToolTipText("Mark_ParameterPreset_Value_Parameters")]
        public object[] Parameters
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Parameters.Select(p => new KeyValuePair<object, object>(p.Key, p.Value)).Cast<object>().ToArray();
                }
                return null;
            }
        }
    }
}
