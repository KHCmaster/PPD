using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Compare_Summary")]
    public partial class CompareFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Compare"; }
        }

        [ToolTipText("TimeSpan_Compare_TimeSpan1")]
        public System.TimeSpan TimeSpan1
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_Compare_TimeSpan2")]
        public System.TimeSpan TimeSpan2
        {
            private get;
            set;
        }

        [ToolTipText("TimeSpan_Compare_Result")]
        public int Result
        {
            get
            {
                SetValue(nameof(TimeSpan1));
                SetValue(nameof(TimeSpan2));
                return System.TimeSpan.Compare(TimeSpan1, TimeSpan2);
            }
        }
    }
}
