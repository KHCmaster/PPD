using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_DistanceSquared_Summary")]
    public partial class DistanceSquaredFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.DistanceSquared"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Vector2 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Vector2 B
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
                return SharpDX.Vector2.DistanceSquared(A, B);
            }
        }
    }
}
