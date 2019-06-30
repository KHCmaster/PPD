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
        public SharpDX.Vector4 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Vector4 B
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
                return SharpDX.Vector4.DistanceSquared(A, B);
            }
        }
    }
}
