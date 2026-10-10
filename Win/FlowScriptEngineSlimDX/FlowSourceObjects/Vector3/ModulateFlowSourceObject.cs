using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Modulate_Summary")]
    public partial class ModulateFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Modulate"; }
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

        [ToolTipText("Vector_Modulate_Value_Summary")]
        public System.Numerics.Vector3 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new System.Numerics.Vector3(
                    A.X * B.X, A.Y * B.Y, A.Z * B.Z);
            }
        }
    }
}
