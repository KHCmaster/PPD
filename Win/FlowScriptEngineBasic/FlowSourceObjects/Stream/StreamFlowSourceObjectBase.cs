namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    public abstract class StreamFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Stream_Stream")]
        public System.IO.Stream Stream
        {
            protected get;
            set;
        }

        protected void SetStream()
        {
            SetValue(nameof(Stream));
        }
    }
}
