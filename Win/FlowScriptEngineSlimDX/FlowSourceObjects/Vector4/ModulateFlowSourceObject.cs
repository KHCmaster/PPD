using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Modulate_Summary")]
    public partial class ModulateFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Modulate"; }
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

        [ToolTipText("Vector_Modulate_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new SharpDX.Vector4(
                    A.X * B.X, A.Y * B.Y, A.Z * B.Z, A.W * B.W);
            }
        }
    }
}
