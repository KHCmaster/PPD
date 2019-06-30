using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_Or_Summary")]
    public partial class OrFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Logic.OR"; }
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

        [ToolTipText("Logic_Or_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                if (A)
                {
                    return true;
                }
                SetValue(nameof(B));
                return B;
            }
        }
    }
}
