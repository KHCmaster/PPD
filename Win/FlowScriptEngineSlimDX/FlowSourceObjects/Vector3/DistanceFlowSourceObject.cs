using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Distance_Summary")]
    public partial class DistanceFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Distance"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Vector3 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Vector3 B
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
                return SharpDX.Vector3.Distance(A, B);
            }
        }
    }
}
