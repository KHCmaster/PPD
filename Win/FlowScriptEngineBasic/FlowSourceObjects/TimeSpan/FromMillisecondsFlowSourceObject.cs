using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_FromMilliseconds_Summary")]
    public partial class FromMillisecondsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.FromMilliseconds"; }
        }

        [ToolTipText("TimeSpan_FromMilliseconds_Milliseconds")]
        public double Milliseconds
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_FromMilliseconds_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(Milliseconds));
                return System.TimeSpan.FromMilliseconds(Milliseconds);
            }
        }
    }
}
