using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Min"; }
        }

        [ToolTipText("DateTime_Min_Value")]
        public System.DateTime Value
        {
            get
            {
                return System.DateTime.MinValue;
            }
        }
    }
}
