using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ContextScope
{
    [ToolTipText("ContextScope_IsDefined_Summary")]
    public partial class IsDefinedFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ContextScope.IsDefined"; }
        }

        [ToolTipText("ContextScope_IsDefined_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_IsDefined_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(ValueName));
                if (ValueName != null)
                {
                    return ContextScope.IsDefined(ValueName) || ContextScope.IsDefinedInParents(ValueName);
                }
                return false;
            }
        }
    }
}
