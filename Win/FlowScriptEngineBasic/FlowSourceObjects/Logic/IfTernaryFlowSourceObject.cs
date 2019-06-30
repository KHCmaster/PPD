using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_IfTernary_Summary")]
    public partial class IfTernaryFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Logic.IfTernary"; }
        }

        [ToolTipText("Logic_IfTernary_Condition")]
        public bool Condition
        {
            private get;
            set;
        }

        [ToolTipText("Logic_IfTernary_A")]
        public object A
        {
            private get;
            set;
        }

        [ToolTipText("Logic_IfTernary_B")]
        public object B
        {
            private get;
            set;
        }

        [ToolTipText("Logic_IfTernary_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Condition));
                if (Condition)
                {
                    SetValue(nameof(A));
                    return A;
                }
                else
                {
                    SetValue(nameof(B));
                    return B;
                }
            }
        }
    }
}
