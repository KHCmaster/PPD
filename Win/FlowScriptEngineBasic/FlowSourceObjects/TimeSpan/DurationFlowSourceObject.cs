using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Duration_Summary")]
    public partial class DurationFlowSourceObject : FlowSourceObjectBase
    {
        System.TimeSpan value;

        public override string Name
        {
            get { return "TimeSpan.Duration"; }
        }

        [ToolTipText("TimeSpan_Duration_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set
            {
                this.value = value.Duration();
            }
        }
    }
}
