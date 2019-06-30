namespace FlowScriptEngineBasic.FlowSourceObjects.Stopwatch
{
    [ToolTipText("Stopwatch_Elapsed_Summary")]
    public partial class ElapsedFlowSourceObject : StopwatchFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stopwatch.Elapsed"; }
        }

        [ToolTipText("Stopwatch_Elapsed_Milliseconds")]
        public double Milliseconds
        {
            get
            {
                if (Stopwatch != null)
                {
                    return Stopwatch.ElapsedMilliseconds;
                }
                return 0;
            }
        }

        [ToolTipText("Stopwatch_Elapsed_ExactSeconds")]
        public double ExactSeconds
        {
            get
            {
                if (Stopwatch != null)
                {
                    return (double)Stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
                }
                return 0;
            }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStopwatch();
            if (Stopwatch != null)
            {
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
