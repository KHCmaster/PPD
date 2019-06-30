using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_IsLeapYear_Summary")]
    public partial class IsLeapYearFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.IsLeapYear"; }
        }

        [ToolTipText("DateTime_IsLeapYear_Year")]
        public int Year
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_IsLeapYear_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(Year));
                if (Year < 1 || Year > 9999)
                {
                    return false;
                }
                return System.DateTime.IsLeapYear(Year);
            }
        }
    }
}
