using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadFloat_Summary")]
    public partial class ReadFloatFlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadFloat"; }
        }

        [ToolTipText("Stream_ReadFloat_Value")]
        public float Value
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
                Value = BitConverter.ToSingle(array, 0);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
