using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_FromMinutes_Summary")]
    public partial class FromMinutesFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.FromMinutes"; }
        }

        [ToolTipText("TimeSpan_FromMinutes_Minutes")]
        public double Minutes
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_FromMinutes_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Minutes));
                return System.TimeSpan.FromMinutes(Minutes);
            }
        }
    }
}
