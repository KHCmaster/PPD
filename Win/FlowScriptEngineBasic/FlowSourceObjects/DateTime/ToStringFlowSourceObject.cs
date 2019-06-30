using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_ToString_Summary")]
    public partial class ToStringFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.ToString"; }
        }

        [ToolTipText("DateTime_ToString_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_ToString_Format")]
        public string Format
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_ToString_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Format));
                return DateTime.ToString(Format);
            }
        }
    }
}
