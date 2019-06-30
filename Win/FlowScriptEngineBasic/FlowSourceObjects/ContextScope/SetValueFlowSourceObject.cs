using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ContextScope
{
    [ToolTipText("ContextScope_SetValue_Summary")]
    public partial class SetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ContextScope.SetValue"; }
        }

        [ToolTipText("ContextScope_SetValue_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_SetValue_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_SetValue_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(ValueName));
            SetValue(nameof(Value));
            if (ValueName != null)
            {
                ContextScope.SetValue(ValueName, Value);
            }
        }
    }
}
