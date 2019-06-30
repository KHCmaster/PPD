using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_ToString_Summary")]
    public partial class ToStringFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.ToString"; }
        }

        [ToolTipText("TimeSpan_ToString_TimeSpan")]
        public System.TimeSpan TimeSpan
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_ToString_Format")]
        public string Format
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_ToString_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(TimeSpan));
                SetValue(nameof(Format));
                return TimeSpan.ToString(Format);
            }
        }
    }
}
