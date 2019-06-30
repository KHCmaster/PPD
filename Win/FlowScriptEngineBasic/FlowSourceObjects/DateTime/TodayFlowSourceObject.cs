using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Today_Summary")]
    public partial class TodayFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Today"; }
        }

        [ToolTipText("DateTime_Today_Today")]
        public System.DateTime Today
        {
            get
            {
                return System.DateTime.Today;
            }
        }
    }
}
