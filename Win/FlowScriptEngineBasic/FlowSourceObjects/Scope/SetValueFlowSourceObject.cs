using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Scope
{
    [ToolTipText("Scope_SetValue_Summary", "Scope_SetValue_Remark")]
    public partial class SetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Scope.SetValue"; }
        }

        [ToolTipText("Scope_SetValue_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("Scope_SetValue_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Scope_SetValue_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(ValueName));
            SetValue(nameof(Value));
            if (ValueName != null)
            {
                Scope.SetValue(ValueName, Value);
            }
        }
    }
}
