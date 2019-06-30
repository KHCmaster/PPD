using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadInt32_Summary")]
    public partial class ReadInt32FlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadInt32"; }
        }

        [ToolTipText("Stream_ReadInt32_Value")]
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
                byte[] array = new byte[4];
                Stream.Read(array, 0, array.Length);
                Value = BitConverter.ToInt32(array, 0);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
