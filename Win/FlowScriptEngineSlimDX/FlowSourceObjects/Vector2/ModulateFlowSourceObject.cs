using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Modulate_Summary")]
    public partial class ModulateFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Modulate"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Vector2 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.Numerics.Vector2 B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Modulate_Value_Summary")]
        public System.Numerics.Vector2 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new System.Numerics.Vector2(
                    A.X * B.X, A.Y * B.Y);
            }
        }
    }
}
