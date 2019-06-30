using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Value"; }
        }

        [ToolTipText("DateTime_Value_Value")]
        public System.DateTime Value
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_Value_Year")]
        public int Year
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Year;
            }
        }

        [ToolTipText("DateTime_Value_Month")]
        public int Month
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Month;
            }
        }

        [ToolTipText("DateTime_Value_Day")]
        public int Day
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Day;
            }
        }

        [ToolTipText("DateTime_Value_Hour")]
        public int Hour
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Hour;
            }
        }

        [ToolTipText("DateTime_Value_Minute")]
        public int Minute
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Minute;
            }
        }

        [ToolTipText("DateTime_Value_Second")]
        public int Second
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Second;
            }
        }

        [ToolTipText("DateTime_Value_Millisecond")]
        public int Millisecond
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Millisecond;
            }
        }

        [ToolTipText("DateTime_Value_Date")]
        public System.DateTime Date
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Date;
            }
        }

        [ToolTipText("DateTime_Value_DayOfWeek")]
        public System.DayOfWeek DayOfWeek
        {
            get
            {
                SetValue(nameof(Value));
                return Value.DayOfWeek;
            }
        }

        [ToolTipText("DateTime_Value_DayOfYear")]
        public int DayOfYear
        {
            get
            {
                SetValue(nameof(Value));
                return Value.DayOfYear;
            }
        }

        [ToolTipText("DateTime_Value_TimeOfDay")]
        public System.TimeSpan TimeOfDay
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TimeOfDay;
            }
        }
    }
}
