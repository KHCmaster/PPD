using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Zero_Summary")]
    public partial class ZeroFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Zero"; }
        }

        [ToolTipText("TimeSpan_Zero_Value")]
        public System.TimeSpan Value
        {
            get
            {
                return System.TimeSpan.Zero;
            }
        }
    }
}
