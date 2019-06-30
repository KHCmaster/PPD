using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_If_Summary", "Logic_If_Remark")]
    public partial class IfFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Logic_If_OnTrue")]
        public event FlowEventHandler OnTrue;

        [ToolTipText("Logic_If_OnFalse")]
        public event FlowEventHandler OnFalse;

        [ToolTipText("Logic_If_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Condition));
            if (Condition)
            {
                FireEvent(OnTrue);
            }
            else
            {
                FireEvent(OnFalse);
            }
        }
        public override string Name
        {
            get
            {
                return "Logic.If";
            }
        }

        [ToolTipText("Logic_If_Condition")]
        public bool Condition
        {
            private get;
            set;
        }
    }
}
