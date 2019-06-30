using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Value"; }
        }

        [ToolTipText("Array_Value_Value")]
        public object[] Value
        {
            get;
            private set;
        }

        [ToolTipText("Array_Value_Length")]
        public int Length
        {
            private get;
            set;
        }

        [ToolTipText("Array_Value_In")]
        public void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Length));
            Value = new object[Length];
        }
    }
}
