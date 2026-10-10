using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadUInt16_Summary")]
    public partial class ReadUInt16FlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadUInt16"; }
        }

        [ToolTipText("Stream_ReadUInt16_Value")]
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
                byte[] array = new byte[2];
                Stream.ReadAtLeast(array, array.Length, throwOnEndOfStream: false);
                Value = BitConverter.ToUInt16(array, 0);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
