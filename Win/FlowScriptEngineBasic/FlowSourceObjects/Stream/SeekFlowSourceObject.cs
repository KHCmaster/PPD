namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_Seek_Summary")]
    public partial class SeekFlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.Seek"; }
        }

        [ToolTipText("Stream_Seek_Offset")]
        public int Offset
        {
            private get;
            set;
        }

        [ToolTipText("Stream_Seek_SeekOrigin")]
        public System.IO.SeekOrigin SeekOrigin
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStream();
            if (Stream != null)
            {
                SetValue(nameof(Offset));
                SetValue(nameof(SeekOrigin));
                Stream.Seek(Offset, SeekOrigin);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
