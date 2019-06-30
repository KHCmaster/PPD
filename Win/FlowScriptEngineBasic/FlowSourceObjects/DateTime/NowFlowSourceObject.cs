using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Now_Summary")]
    public partial class NowFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Now"; }
        }

        [ToolTipText("DateTime_Now_Now")]
        public System.DateTime Now
        {
            get
            {
                return System.DateTime.Now;
            }
        }
    }
}
