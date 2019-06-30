using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddHours_Summary")]
    public partial class AddHoursFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddHours"; }
        }

        [ToolTipText("DateTime_AddHours_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddHours_Hours")]
        public double Hours
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddHours_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Hours));
                return DateTime.AddHours(Hours);
            }
        }
    }
}
