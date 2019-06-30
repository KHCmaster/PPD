using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_FromHours_Summary")]
    public partial class FromHoursFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.FromHours"; }
        }

        [ToolTipText("TimeSpan_FromHours_Hours")]
        public double Hours
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_FromHours_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Hours));
                return System.TimeSpan.FromHours(Hours);
            }
        }
    }
}
