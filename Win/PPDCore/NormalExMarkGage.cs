using PPDFramework;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDCore
{
    class NormalExMarkGage : GameComponent
    {
        const float alpha = 0.7f;
        VertexInfo vertices;
        PictureObject trace;

        public int DisplayCount
        {
            get;
            set;
        }

        public NormalExMarkGage(PPDDevice device, PictureObject trace, Vector2[] circlePoints) : base(device)
        {
            this.trace = trace;
            vertices = device.GetModule<ShaderCommon>().CreateVertex(circlePoints.Length);
            ColoredTexturedVertex[] dataVertices = new ColoredTexturedVertex[circlePoints.Length];
            var uv = trace.ImageResource.GetActualUV(new Vector2(alpha, 0.5f)) + trace.ImageResource.HalfPixel;
            var color = PPDColors.White.ToBgra();
            for (int i = 0; i < dataVertices.Length; i++)
            {
                dataVertices[i].Position = new Vector3(circlePoints[i].X, circlePoints[i].Y, 0.5f);
                dataVertices[i].TextureCoordinates = uv;
                dataVertices[i].Color = color;
            }
            vertices.Write(dataVertices);
        }

        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = trace.ImageResource.Texture;
            alphaBlendContext.Vertex = vertices;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleStrip, DisplayCount * 2 - 2, 0, DisplayCount * 2);
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return DisplayCount > 0;
        }

        protected override void DisposeResource()
        {
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }
        }
    }
}
