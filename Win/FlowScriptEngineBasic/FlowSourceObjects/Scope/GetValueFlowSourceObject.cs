using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Scope
{
    [ToolTipText("Scope_GetValue_Summary", "Scope_GetValue_Remark")]
    public partial class GetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Scope.GetValue"; }
        }

        [ToolTipText("Scope_GetValue_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("Scope_GetValue_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(ValueName));
                if (ValueName != null)
                {
                    return Scope.GetValue(ValueName);
                }
                return null;
            }
        }
    }
}
