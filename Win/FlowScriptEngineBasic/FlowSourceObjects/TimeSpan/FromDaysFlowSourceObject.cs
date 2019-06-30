using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_FromDays_Summary")]
    public partial class FromDaysFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.FromDays"; }
        }

        [ToolTipText("TimeSpan_FromDays_Days")]
        public double Days
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_FromDays_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Days));
                return System.TimeSpan.FromDays(Days);
            }
        }
    }
}
