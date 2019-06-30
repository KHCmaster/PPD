using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddYears_Summary")]
    public partial class AddYearsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddYears"; }
        }

        [ToolTipText("DateTime_AddYears_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddYears_Years")]
        public int Years
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddYears_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Years));
                return DateTime.AddYears(Years);
            }
        }
    }
}
