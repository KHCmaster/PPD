using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_FromSeconds_Summary")]
    public partial class FromSecondsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.FromSeconds"; }
        }

        [ToolTipText("TimeSpan_FromSeconds_Seconds")]
        public double Seconds
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_FromSeconds_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Seconds));
                return System.TimeSpan.FromSeconds(Seconds);
            }
        }
    }
}
