using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddMinutes_Summary")]
    public partial class AddMinutesFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddMinutes"; }
        }

        [ToolTipText("DateTime_AddMinutes_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddMinutes_Minutes")]
        public double Minutes
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddMinutes_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Minutes));
                return DateTime.AddMinutes(Minutes);
            }
        }
    }
}
