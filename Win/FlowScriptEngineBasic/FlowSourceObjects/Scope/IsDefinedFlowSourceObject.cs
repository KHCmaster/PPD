using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Scope
{
    [ToolTipText("Scope_IsDefined_Summary")]
    public partial class IsDefinedFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Scope.IsDefined"; }
        }

        [ToolTipText("Scope_IsDefined_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("Scope_IsDefined_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(ValueName));
                if (ValueName != null)
                {
                    return Scope.IsDefined(ValueName) || Scope.IsDefinedInParents(ValueName);
                }
                return false;
            }
        }
    }
}
