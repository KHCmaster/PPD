using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_FromWithSecond_Summary")]
    public partial class FromWithSecondFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.From"; }
        }

        [ToolTipText("DateTime_From_Year")]
        public int Year
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_From_Month")]
        public int Month
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_From_Day")]
        public int Day
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_From_Hour")]
        public int Hour
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_From_Minute")]
        public int Minute
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_From_Second")]
        public int Second
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_From_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(Year));
                SetValue(nameof(Month));
                SetValue(nameof(Day));
                SetValue(nameof(Hour));
                SetValue(nameof(Minute));
                SetValue(nameof(Second));
                try
                {
                    return new System.DateTime(Year, Month, Day, Hour,
                        Minute, Second);
                }
                catch
                {
                    return System.DateTime.MinValue;
                }
            }
        }
    }
}
