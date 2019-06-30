using FlowScriptEngine;
using System;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Max"; }
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

        [ToolTipText("Vector_Max_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new SharpDX.Vector3(
                    Math.Max(A.X, B.X),
                    Math.Max(A.Y, B.Y),
                    Math.Max(A.Z, B.Z));
            }
        }
    }
}
