using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddDays_Summary")]
    public partial class AddDaysFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddDays"; }
        }

        [ToolTipText("DateTime_AddDays_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddDays_Days")]
        public double Days
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddDays_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Days));
                return DateTime.AddDays(Days);
            }
        }
    }
}
