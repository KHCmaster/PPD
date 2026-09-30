using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_DistanceSquared_Summary")]
    public partial class DistanceSquaredFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.DistanceSquared"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Vector4 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.Numerics.Vector4 B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_DistanceSquared_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Numerics.Vector4.DistanceSquared(A, B);
            }
        }
    }
}
