using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_UtcNow_Summary")]
    public partial class UtcNowFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.UtcNow"; }
        }

        [ToolTipText("DateTime_UtcNow_UtcNow")]
        public System.DateTime UtcNow
        {
            get
            {
                return System.DateTime.UtcNow;
            }
        }
    }
}
