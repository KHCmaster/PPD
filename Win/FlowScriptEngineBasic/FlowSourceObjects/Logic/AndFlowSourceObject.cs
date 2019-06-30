using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_And_Summary", "Logic_And_Remark")]
    public partial class AndFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Logic.AND"; }
        }

        [ToolTipText("FirstArgument")]
        public bool A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public bool B
        {
            private get;
            set;
        }

        [ToolTipText("Logic_And_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                if (!A)
                {
                    return false;
                }
                SetValue(nameof(B));
                return B;
            }
        }
    }
}
