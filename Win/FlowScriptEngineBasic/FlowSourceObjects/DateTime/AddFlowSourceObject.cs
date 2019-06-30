using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Add_Summary")]
    public partial class AddFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Add"; }
        }

        [ToolTipText("DateTime_Add_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_Add_Time")]
        public System.TimeSpan Time
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_Add_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Time));
                return DateTime.Add(Time);
            }
        }
    }
}
