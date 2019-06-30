using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Dot_Summary")]
    public partial class DotFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Dot"; }
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

        [ToolTipText("Vector_Dot_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return SharpDX.Vector2.Dot(A, B);
            }
        }
    }
}
