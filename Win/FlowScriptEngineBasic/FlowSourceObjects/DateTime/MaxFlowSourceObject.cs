using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Max"; }
        }

        [ToolTipText("DateTime_Max_Value")]
        public System.DateTime Value
        {
            get
            {
                return System.DateTime.MaxValue;
            }
        }
    }
}
