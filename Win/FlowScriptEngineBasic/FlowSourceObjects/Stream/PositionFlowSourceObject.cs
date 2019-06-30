using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_Position_Summary")]
    public partial class PositionFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.Position"; }
        }

        [ToolTipText("Stream_Position_Stream")]
        public System.IO.Stream Stream
        {
            protected get;
            set;
        }

        protected void SetStream()
        {
            SetValue(nameof(Stream));
        }

        [ToolTipText("Stream_Position_Value")]
        public int Value
        {
            get
            {
                SetStream();
                if (Stream != null)
                {
                    return (int)Stream.Position;
                }
                return 0;
            }
        }
    }
}
