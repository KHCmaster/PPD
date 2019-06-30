using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.GlobalScope
{
    [ToolTipText("GlobalScope_IsDefined_Summary")]
    public partial class IsDefinedFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "GlobalScope.IsDefined"; }
        }

        [ToolTipText("GlobalScope_IsDefined_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_IsDefined_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(ValueName));
                if (ValueName != null)
                {
                    return GlobalScope.IsDefined(ValueName) || GlobalScope.IsDefinedInParents(ValueName);
                }
                return false;
            }
        }
    }
}
