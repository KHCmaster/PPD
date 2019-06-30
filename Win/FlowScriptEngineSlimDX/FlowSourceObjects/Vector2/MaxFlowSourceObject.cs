using FlowScriptEngine;
using System;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Max"; }
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

        [ToolTipText("Vector_Max_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new SharpDX.Vector2(
                    Math.Max(A.X, B.X),
                    Math.Max(A.Y, B.Y));
            }
        }
    }
}
