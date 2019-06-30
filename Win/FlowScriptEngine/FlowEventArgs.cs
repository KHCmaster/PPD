namespace FlowScriptEngine
{
    public delegate void FlowEventHandler(FlowEventArgs e);
    public class FlowEventArgs
    {
        internal FlowEventArgs(FlowSourceObjectBase source, bool isBreakUsed)
        {
            Source = source;
            IsBreakUsed = isBreakUsed;
        }

        public FlowSourceObjectBase Source
        {
            get;
            private set;
        }

        public bool IsBreakUsed
        {
            get;
            private set;
        }
    }
}
