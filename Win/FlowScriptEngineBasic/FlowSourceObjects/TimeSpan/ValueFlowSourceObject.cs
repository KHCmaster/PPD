using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Value"; }
        }

        [ToolTipText("TimeSpan_Value_Value")]
        public System.TimeSpan Value
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_Value_Days")]
        public int Days
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Days;
            }
        }

        [ToolTipText("TimeSpan_Value_Hours")]
        public int Hours
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Hours;
            }
        }

        [ToolTipText("TimeSpan_Value_Milliseconds")]
        public int Milliseconds
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Milliseconds;
            }
        }

        [ToolTipText("TimeSpan_Value_Minutes")]
        public int Minutes
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Minutes;
            }
        }

        [ToolTipText("TimeSpan_Value_Seconds")]
        public int Seconds
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Seconds;
            }
        }

        [ToolTipText("TimeSpan_Value_TotalDays")]
        public double TotalDays
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TotalDays;
            }
        }

        [ToolTipText("TimeSpan_Value_TotalHours")]
        public double TotalHours
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TotalHours;
            }
        }

        [ToolTipText("TimeSpan_Value_TotalMilliseconds")]
        public double TotalMilliseconds
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TotalMilliseconds;
            }
        }

        [ToolTipText("TimeSpan_Value_TotalMinutes")]
        public double TotalMinutes
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TotalMinutes;
            }
        }

        [ToolTipText("TimeSpan_Value_TotalSeconds")]
        public double TotalSeconds
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TotalSeconds;
            }
        }


    }
}
