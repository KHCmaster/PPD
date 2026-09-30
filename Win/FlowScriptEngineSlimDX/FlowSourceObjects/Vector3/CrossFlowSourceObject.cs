using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Cross_Summary")]
    public partial class CrossFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Cross"; }
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

        [ToolTipText("Vector_Cross_Value_Summary")]
        public System.Numerics.Vector3 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Numerics.Vector3.Cross(A, B);
            }
        }
    }
}
