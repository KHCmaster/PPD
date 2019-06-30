using PPDFramework;
using SharpDX;

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
                var b = (Value.Color >> 24) & 0xff;
                var g = (Value.Color >> 16) & 0xff;
                var r = (Value.Color >> 8) & 0xff;
                var a = Value.Color & 0xff;
                return new Color4(r, g, b, a);
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
