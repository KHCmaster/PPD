using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_From_Summary")]
    public partial class FromFlowSourceObject : FlowSourceObjectBase
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

        [ToolTipText("DateTime_From_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(Year));
                SetValue(nameof(Month));
                SetValue(nameof(Day));
                try
                {
                    return new System.DateTime(Year, Month, Day);
                }
                catch
                {
                    return System.DateTime.MinValue;
                }
            }
        }
    }
}
