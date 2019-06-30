using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.Min"; }
        }

        [ToolTipText("Min_Value")]
        public int Min
        {
            get
            {
                return int.MinValue;
            }
        }
    }
}
