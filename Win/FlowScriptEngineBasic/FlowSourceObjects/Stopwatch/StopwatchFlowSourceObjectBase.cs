namespace FlowScriptEngineBasic.FlowSourceObjects.Stopwatch
{
    public abstract class StopwatchFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Stopwatch_Stopwatch")]
        public System.Diagnostics.Stopwatch Stopwatch
        {
            protected get;
            set;
        }

        protected void SetStopwatch()
        {
            SetValue(nameof(Stopwatch));
        }
    }
}
