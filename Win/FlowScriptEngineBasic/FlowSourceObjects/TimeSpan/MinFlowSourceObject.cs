using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Min"; }
        }

        [ToolTipText("TimeSpan_Min_Value")]
        public System.TimeSpan Value
        {
            get
            {
                return System.TimeSpan.MinValue;
            }
        }
    }
}
