using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Compare_Summary")]
    public partial class CompareFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Compare"; }
        }

        [ToolTipText("DateTime_Compare_DateTime1")]
        public System.DateTime DateTime1
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_Compare_DateTime2")]
        public System.DateTime DateTime2
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_Compare_Result")]
        public int Result
        {
            get
            {
                SetValue(nameof(DateTime1));
                SetValue(nameof(DateTime2));
                return System.DateTime.Compare(DateTime1, DateTime2);
            }
        }
    }
}
