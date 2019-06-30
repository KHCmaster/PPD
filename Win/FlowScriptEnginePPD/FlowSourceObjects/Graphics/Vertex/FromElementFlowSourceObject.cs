using FlowScriptEngine;
using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Vertex
{
    [ToolTipText("Vertex_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Vertex.FromElement"; }
        }

        [ToolTipText("Vertex_FromElement_Color")]
        public Color4 Color
        {
            private get;
            set;
        }

        [ToolTipText("Vertex_FromElement_Position")]
        public Vector3 Position
        {
            private get;
            set;
        }

        [ToolTipText("Vertex_FromElement_TextureCoordinates")]
        public Vector2 TextureCoordinates
        {
            private get;
            set;
        }

        [ToolTipText("Vertex_FromElement_Value")]
        public ColoredTexturedVertex Value
        {
            get
            {
                SetValue(nameof(Color));
                SetValue(nameof(Position));
                SetValue(nameof(TextureCoordinates));
                return new ColoredTexturedVertex(Position, Color, TextureCoordinates);
            }
        }
    }
}
