using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_SubtractDateTime_Summary")]
    public partial class SubtractDateTimeFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Subtract"; }
        }

        [ToolTipText("DateTime_SubtractDateTime_DateTime1")]
        public System.DateTime DateTime1
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_SubtractDateTime_DateTime2")]
        public System.DateTime DateTime2
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_SubtractDateTime_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(DateTime1));
                SetValue(nameof(DateTime2));
                return DateTime1 - DateTime2;
            }
        }
    }
}
