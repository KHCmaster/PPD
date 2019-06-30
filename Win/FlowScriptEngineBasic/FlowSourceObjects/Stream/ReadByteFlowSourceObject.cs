namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadByte_Summary")]
    public partial class ReadByteFlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadByte"; }
        }

        [ToolTipText("Stream_ReadByte_Value")]
        public int Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStream();
            if (Stream != null)
            {
                Value = Stream.ReadByte();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
