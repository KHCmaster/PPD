using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Add_Summary")]
    public partial class AddFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Add"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Vector3 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.Numerics.Vector3 B
        {
            private get;
            set;
        }

        [ToolTipText("Add_Value")]
        public System.Numerics.Vector3 Value
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
