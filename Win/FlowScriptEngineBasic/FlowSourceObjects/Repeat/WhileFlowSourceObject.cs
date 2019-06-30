using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat
{
    [ToolTipText("Repeat_While_Summary")]
    public partial class WhileFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Repeat_While_Loop")]
        public event FlowEventHandler Loop;
        private event FlowEventHandler LoopEnd;
        public WhileFlowSourceObject()
        {
            LoopEnd += WhileFlowSourceObject_LoopEnd;
        }

        void WhileFlowSourceObject_LoopEnd(FlowEventArgs e)
        {
            if (e.IsBreakUsed)
            {
                return;
            }

            SetValue(nameof(Condition));
            if (Condition)
            {
                FireEvent(Loop, true);
                FireEvent(LoopEnd, true, true);
            }
        }

        public override string Name
        {
            get { return "Repeat.While"; }
        }

        [ToolTipText("Repeat_While_Condition")]
        public bool Condition
        {
            private get;
            set;
        }

        [ToolTipText("Repeat_While_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Condition));
            if (Condition)
            {
                FireEvent(Loop, true);
                FireEvent(LoopEnd, true, true);
            }
        }
    }
}
