using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Negate_Summary")]
    public partial class NegateFlowSourceObject : FlowSourceObjectBase
    {
        System.TimeSpan value;

        public override string Name
        {
            get { return "TimeSpan.Negate"; }
        }

        [ToolTipText("TimeSpan_Negate_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set
            {
                this.value = value.Negate();
            }
        }
    }
}
