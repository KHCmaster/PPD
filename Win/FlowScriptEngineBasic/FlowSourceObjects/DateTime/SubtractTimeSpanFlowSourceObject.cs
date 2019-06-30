using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_SubtractTimeSpan_Summary")]
    public partial class SubtractTimeSpanFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Subtract"; }
        }

        [ToolTipText("DateTime_SubtractTimeSpan_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_SubtractTimeSpan_Time")]
        public System.TimeSpan Time
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_SubtractTimeSpan_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Time));
                return DateTime - Time;
            }
        }
    }
}
