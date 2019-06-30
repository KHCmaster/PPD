using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Add_Summary")]
    public partial class AddFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Add"; }
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

        [ToolTipText("Add_Value")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A + B;
            }
        }
    }
}
