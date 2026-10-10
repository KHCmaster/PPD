using PPDFramework;
using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;
using Color = Vortice.Mathematics.Color;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Vertex
{
    [ToolTipText("Vertex_Value_Summary")]
    public partial class ValueFlowSourceObject : TemplateStructValueFlowSourceObject<ColoredTexturedVertex>
    {
        public override string Name
        {
            get { return "PPD.Graphics.Vertex.Value"; }
        }

        [ToolTipText("Vertex_Value_Color")]
        public Color4 Color
        {
            get
            {
                SetValue(nameof(Value));
                // 頂点の色は ToBgraInt の形（上位から A, R, G, B）。元は並びを取り違え、0 から 255 のまま返していた
                var a = (Value.Color >> 24) & 0xff;
                var r = (Value.Color >> 16) & 0xff;
                var g = (Value.Color >> 8) & 0xff;
                var b = Value.Color & 0xff;
                return new Color4(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }

        [ToolTipText("Vertex_Value_Position")]
        public Vector3 Position
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Position;
            }
        }

        [ToolTipText("Vertex_Value_TextureCoordinates")]
        public Vector2 TextureCoordinates
        {
            get
            {
                SetValue(nameof(Value));
                return Value.TextureCoordinates;
            }
        }
    }
}
