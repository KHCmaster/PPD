using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.FromElement"; }
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

        [ToolTipText("Vector_FromElement_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                SetValue(nameof(X));
                SetValue(nameof(Y));
                return new SharpDX.Vector2(X, Y);
            }
        }
    }
}
