using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.FromElement"; }
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

        [ToolTipText("Vector_FromElement_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(X));
                SetValue(nameof(Y));
                SetValue(nameof(Z));
                return new SharpDX.Vector3(X, Y, Z);
            }
        }
    }
}
