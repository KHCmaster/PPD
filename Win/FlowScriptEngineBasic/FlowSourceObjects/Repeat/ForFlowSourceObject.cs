using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat
{
    [ToolTipText("Repeat_For_Summary")]
    public partial class ForFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Repeat_For_Init")]
        public event FlowEventHandler Init;
        [ToolTipText("Repeat_For_Loop")]
        public event FlowEventHandler Loop;
        [ToolTipText("Repeat_For_LoopEnd")]
        public event FlowEventHandler LoopEnd;

        private event FlowEventHandler InitEnd;
        private event FlowEventHandler LoopEndInternal;

        public ForFlowSourceObject()
        {
            InitEnd += ForFlowSourceObject_InitEnd;
            LoopEndInternal += ForFlowSourceObject_LoopEndInternal;
        }

        public override string Name
        {
            get { return "Repeat.For"; }
        }

        void ForFlowSourceObject_InitEnd(FlowEventArgs e)
        {
            if (e.IsBreakUsed)
            {
                return;
            }

            SetValue(nameof(Condition));
            if (Condition)
            {
                FireEvent(Loop, true);
                FireEvent(LoopEndInternal, true, true);
                FireEvent(LoopEnd, true);
                FireEvent(InitEnd, true, true);
            }
        }

        void ForFlowSourceObject_LoopEndInternal(FlowEventArgs e)
        {
            if (e.IsBreakUsed)
            {
                BreakLoop();
            }
        }

        [ToolTipText("Repeat_For_Condition")]
        public bool Condition
        {
            private get;
            set;
        }

        [ToolTipText("Repeat_For_In")]
        public void In(FlowEventArgs e)
        {
            FireEvent(Init, true);
            FireEvent(InitEnd, true);
        }
    }
}
