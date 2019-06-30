using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Equal_Summary")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Equal"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Quaternion A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Quaternion B
        {
            private get;
            set;
        }

        [ToolTipText("Equal_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A == B;
            }
        }
    }
}
