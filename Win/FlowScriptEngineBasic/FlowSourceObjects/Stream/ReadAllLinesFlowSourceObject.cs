using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadAllLines_Summary")]
    public partial class ReadAllLinesFlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadAllLines"; }
        }

        [ToolTipText("Stream_ReadAllLines_Encoding")]
        public EncodingType Encoding
        {
            private get;
            set;
        }

        [ToolTipText("Stream_ReadAllLines_Value")]
        public List<object> Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetStream();
            SetValue(nameof(Encoding));
            if (Stream != null)
            {
                byte[] array = new byte[Stream.Length - Stream.Position];
                Stream.Read(array, 0, array.Length);
                var str = EncodingUtility.GetEncoding(Encoding).GetString(array);
                Value = new List<object>(str.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
