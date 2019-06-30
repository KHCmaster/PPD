using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stopwatch
{
    [ToolTipText("Stopwatch_Start_Value")]
    public partial class StartFlowSourceObject : StopwatchFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stopwatch.Start"; }
        }

        public override void In(FlowEventArgs e)
        {
            SetStopwatch();
            if (Stopwatch != null)
            {
                Stopwatch.Start();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
