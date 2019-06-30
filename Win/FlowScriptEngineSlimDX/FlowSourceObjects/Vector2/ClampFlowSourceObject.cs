using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Clamp_Summary")]
    public partial class ClampFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Clamp"; }
        }

        [ToolTipText("Vector_Clamp_Min_Summary")]
        public SharpDX.Vector2 Min
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Clamp_Max_Summary")]
        public SharpDX.Vector2 Max
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Clamp_Vector_Summary")]
        public SharpDX.Vector2 Vector
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Clamp_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                SetValue(nameof(Min));
                SetValue(nameof(Max));
                SetValue(nameof(Vector));
                return SharpDX.Vector2.Clamp(Vector, Min, Max);
            }
        }
    }
}
