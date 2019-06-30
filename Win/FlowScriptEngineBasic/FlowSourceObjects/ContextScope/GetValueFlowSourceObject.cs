using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ContextScope
{
    [ToolTipText("ContextScope_GetValue_Summary")]
    public partial class GetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ContextScope.GetValue"; }
        }

        [ToolTipText("ContextScope_GetValue_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_GetValue_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(ValueName));
                if (ValueName != null)
                {
                    return ContextScope.GetValue(ValueName);
                }
                return null;
            }
        }
    }
}
