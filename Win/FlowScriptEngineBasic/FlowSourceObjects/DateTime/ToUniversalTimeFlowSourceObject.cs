using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_ToUniversalTime_Summary")]
    public partial class ToUniversalTimeFlowSourceObject : FlowSourceObjectBase
    {
        private System.DateTime value;

        public override string Name
        {
            get { return "DateTime.ToUniversalTime"; }
        }

        [ToolTipText("DateTime_ToUniversalTime_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set
            {
                this.value = value.ToUniversalTime();
            }
        }
    }
}
