using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Max"; }
        }

        [ToolTipText("TimeSpan_Max_Value")]
        public System.TimeSpan Value
        {
            get
            {
                return System.TimeSpan.MaxValue;
            }
        }
    }
}
