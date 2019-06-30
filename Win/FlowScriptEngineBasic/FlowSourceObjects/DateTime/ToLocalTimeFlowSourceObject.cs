using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_ToLocalTime_Summary")]
    public partial class ToLocalTimeFlowSourceObject : FlowSourceObjectBase
    {
        private System.DateTime value;

        public override string Name
        {
            get { return "DateTime.ToLocalTime"; }
        }

        [ToolTipText("DateTime_ToLocalTime_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set
            {
                this.value = value.ToLocalTime();
            }
        }
    }
}
