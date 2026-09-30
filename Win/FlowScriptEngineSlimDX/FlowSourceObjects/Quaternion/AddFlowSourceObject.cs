using PPDMath;
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
        public System.Numerics.Quaternion A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.Numerics.Quaternion B
        {
            private get;
            set;
        }

        [ToolTipText("Add_Value")]
        public System.Numerics.Quaternion Value
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
