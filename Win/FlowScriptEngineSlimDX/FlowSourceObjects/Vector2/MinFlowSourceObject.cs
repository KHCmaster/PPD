using PPDMath;
using FlowScriptEngine;
using System;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Min"; }
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

        [ToolTipText("Vector_Min_Value_Summary")]
        public System.Numerics.Vector2 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new System.Numerics.Vector2(
                    Math.Min(A.X, B.X),
                    Math.Min(A.Y, B.Y));
            }
        }
    }
}
