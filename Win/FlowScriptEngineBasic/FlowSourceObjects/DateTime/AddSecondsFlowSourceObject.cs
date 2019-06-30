using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddSeconds_Summary")]
    public partial class AddSecondsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddSeconds"; }
        }

        [ToolTipText("DateTime_AddSeconds_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddSeconds_Seconds")]
        public double Seconds
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddSeconds_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Seconds));
                return DateTime.AddSeconds(Seconds);
            }
        }
    }
}
