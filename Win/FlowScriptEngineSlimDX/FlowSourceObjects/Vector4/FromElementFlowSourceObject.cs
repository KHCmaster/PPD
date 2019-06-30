using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.FromElement"; }
        }

        [ToolTipText("Vector_FromElement_X_Summary")]
        public float X
        {
            private get;
            set;
        }

        [ToolTipText("Vector_FromElement_Y_Summary")]
        public float Y
        {
            private get;
            set;
        }

        [ToolTipText("Vector_FromElement_Z_Summary")]
        public float Z
        {
            private get;
            set;
        }

        [ToolTipText("Vector_FromElement_W_Summary")]
        public float W
        {
            private get;
            set;
        }

        [ToolTipText("Vector_FromElement_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                SetValue(nameof(X));
                SetValue(nameof(Y));
                SetValue(nameof(Z));
                SetValue(nameof(W));
                return new SharpDX.Vector4(X, Y, Z, W);
            }
        }
    }
}
