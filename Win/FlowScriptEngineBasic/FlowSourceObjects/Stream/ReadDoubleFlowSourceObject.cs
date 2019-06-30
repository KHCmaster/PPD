using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadDouble_Summary")]
    public partial class ReadDoubleFlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadDouble"; }
        }

        [ToolTipText("Stream_ReadDouble_Value")]
        public double Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStream();
            if (Stream != null)
            {
                byte[] array = new byte[8];
                Stream.Read(array, 0, array.Length);
                Value = BitConverter.ToDouble(array, 0);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
