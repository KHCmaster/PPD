using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Dot_Summary")]
    public partial class DotFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Dot"; }
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

        [ToolTipText("Vector_Dot_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Numerics.Vector4.Dot(A, B);
            }
        }
    }
}
