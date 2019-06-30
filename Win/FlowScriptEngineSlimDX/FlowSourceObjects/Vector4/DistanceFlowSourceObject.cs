using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Distance_Summary")]
    public partial class DistanceFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Distance"; }
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

        [ToolTipText("Vector_Distance_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return SharpDX.Vector4.Distance(A, B);
            }
        }
    }
}
