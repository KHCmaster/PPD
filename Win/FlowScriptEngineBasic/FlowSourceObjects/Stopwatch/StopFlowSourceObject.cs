namespace FlowScriptEngineBasic.FlowSourceObjects.Stopwatch
{
    [ToolTipText("Stopwatch_Stop_Summary")]
    public partial class StopFlowSourceObject : StopwatchFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stopwatch.Stop"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStopwatch();
            if (Stopwatch != null)
            {
                Stopwatch.Stop();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
