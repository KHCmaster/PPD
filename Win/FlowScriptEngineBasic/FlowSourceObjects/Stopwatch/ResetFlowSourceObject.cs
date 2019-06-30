namespace FlowScriptEngineBasic.FlowSourceObjects.Stopwatch
{
    [ToolTipText("Stopwatch_Reset_Summary")]
    public partial class ResetFlowSourceObject : StopwatchFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stopwatch.Reset"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStopwatch();
            if (Stopwatch != null)
            {
                Stopwatch.Reset();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
