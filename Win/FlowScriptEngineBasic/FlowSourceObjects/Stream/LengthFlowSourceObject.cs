using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.Length"; }
        }

        [ToolTipText("Stream_Length_Stream")]
        public System.IO.Stream Stream
        {
            protected get;
            set;
        }

        protected void SetStream()
        {
            SetValue(nameof(Stream));
        }

        [ToolTipText("Stream_Length_Value")]
        public int Value
        {
            get
            {
                SetStream();
                if (Stream != null)
                {
                    return (int)Stream.Length;
                }
                return 0;
            }
        }
    }
}
