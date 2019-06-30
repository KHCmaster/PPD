using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.GlobalScope
{
    [ToolTipText("GlobalScope_SetValue_Summary")]
    public partial class SetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "GlobalScope.SetValue"; }
        }

        [ToolTipText("GlobalScope_SetValue_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_SetValue_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_SetValue_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(ValueName));
            SetValue(nameof(Value));
            if (ValueName != null)
            {
                GlobalScope.SetValue(ValueName, Value);
            }
        }
    }
}
