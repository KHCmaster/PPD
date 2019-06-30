using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_AddMilliseconds_Summary")]
    public partial class AddMillisecondsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.AddMilliseconds"; }
        }

        [ToolTipText("DateTime_AddMilliseconds_DateTime")]
        public System.DateTime DateTime
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddMilliseconds_Milliseconds")]
        public double Milliseconds
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_AddMilliseconds_Value")]
        public System.DateTime Value
        {
            get
            {
                SetValue(nameof(DateTime));
                SetValue(nameof(Milliseconds));
                return DateTime.AddMilliseconds(Milliseconds);
            }
        }
    }
}
