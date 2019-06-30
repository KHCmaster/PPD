using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.GlobalScope
{
    [ToolTipText("GlobalScope_GetValue_Summary")]
    public partial class GetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "GlobalScope.GetValue"; }
        }

        [ToolTipText("GlobalScope_GetValue_ValueName")]
        public string ValueName
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_GetValue_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(ValueName));
                if (ValueName != null)
                {
                    return GlobalScope.GetValue(ValueName);
                }
                return null;
            }
        }
    }
}
