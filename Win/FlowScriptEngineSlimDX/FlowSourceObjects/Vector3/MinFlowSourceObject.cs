using FlowScriptEngine;
using System;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Min"; }
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

        [ToolTipText("Vector_Min_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new SharpDX.Vector3(
                    Math.Min(A.X, B.X),
                    Math.Min(A.Y, B.Y),
                    Math.Min(A.Z, B.Z));
            }
        }
    }
}
