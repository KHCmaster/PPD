using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat
{
    [ToolTipText("Repeat_DoWhile_Summary", "Repeat_DoWhile_Remark")]
    public partial class DoWhileFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Repeat_DoWhile_Loop")]
        public event FlowEventHandler Loop;
        private event FlowEventHandler LoopEnd;
        public DoWhileFlowSourceObject()
        {
            LoopEnd += DoWhileFlowSourceObject_LoopEnd;
        }

        void DoWhileFlowSourceObject_LoopEnd(FlowEventArgs e)
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
            get { return "Repeat.DoWhile"; }
        }

        [ToolTipText("Repeat_DoWhile_Condition")]
        public bool Condition
        {
            private get;
            set;
        }

        [ToolTipText("Repeat_DoWhile_In")]
        public void In(FlowEventArgs e)
        {
            FireEvent(Loop, true);
            FireEvent(LoopEnd, true, true);
        }
    }
}
