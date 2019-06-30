using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddMonths_Summary")]
    public partial class AddMonthsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddMonths"; }
        }

        [ToolTipText("DateTime_AddMonths_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddMonths_Months")]
        public int Months
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddMonths_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Months));
                return DateTime.AddMonths(Months);
            }
        }
    }
}
