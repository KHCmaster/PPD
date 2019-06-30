using System.IO;

namespace FlowScriptEngineBasic.FlowSourceObjects.Stream
{
    [ToolTipText("Stream_ReadAllText_Summary")]
    public partial class ReadAllTextFlowSourceObject : StreamFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Stream.ReadAllText"; }
        }

        [ToolTipText("Stream_ReadAllText_Encoding")]
        public EncodingType Encoding
        {
            private get;
            set;
        }

        [ToolTipText("Stream_ReadAllText_Value")]
        public string Value
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
                using (var reader = new StreamReader(Stream, EncodingUtility.GetEncoding(Encoding)))
                {
                    Value = reader.ReadToEnd();
                }
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
