using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.Max"; }
        }

        [ToolTipText("Max_Value")]
        public int Value
        {
            get
            {
                return int.MaxValue;
            }
        }
    }
}
