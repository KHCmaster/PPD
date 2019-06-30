using PPDFramework;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDCore
{
    class ColorMarkInfo : GameComponent
    {
        VertexInfo vertices1;
        VertexInfo vertices2;
        PictureObject trace;
        int drawVerticesCount1;
        int drawVerticesCount2;

        public int VerticesCount
        {
            get;
            set;
        }

        public Vector2 BasePosition
        {
            get;
            private set;
        }

        public Vector2[] Pos1
        {
            get;
            private set;
        }

        public Vector2[] Pos2
        {
            get;
            private set;
        }

        public GameComponent ColorMark
        {
            get;
            private set;
        }

        public bool DrawTrace
        {
            get;
            set;
        }

        public ColorMarkInfo(PPDDevice device, GameComponent colorMark, Vector2 basePosition, PictureObject trace) : base(device)
        {
            ColorMark = colorMark;
            BasePosition = basePosition;
            if (trace == null)
            {

            }
            this.trace = trace;
            Pos1 = new Vector2[40];
            Pos2 = new Vector2[40];
            vertices1 = device.GetModule<ShaderCommon>().CreateVertex(80);
            vertices2 = device.GetModule<ShaderCommon>().CreateVertex(80);
        }

        protected override void UpdateImpl()
        {
            drawVerticesCount1 = UpdateVerticesImpl(vertices1, VerticesCount, Pos1);
            drawVerticesCount2 = UpdateVerticesImpl(vertices2, VerticesCount, Pos2);
        }

        private int UpdateVerticesImpl(VertexInfo vertices, int verticesCount, Vector2[] pos)
        {
            ColoredTexturedVertex[] dataVertices = new ColoredTexturedVertex[verticesCount * 2];
            if (verticesCount < 2)
            {
                return 0;
            }
            Vector2 v, vD;
            var vN1 = Vector2.Zero;
            var vN2 = Vector2.Zero;
            float width = 2f;
            float alpha = 0f;
            var drawVerticesCount = 0;
            for (var i = 0; i < verticesCount; i++)
            {
                if (i < (verticesCount - 1))
                {
                    v.X = pos[i + 1].X - pos[i].X;
                    v.Y = pos[i + 1].Y - pos[i].Y;

                    if (v.X == 0 && v.Y == 0)
                    {
                        continue;
                    }

                    vD = Vector2.Normalize(v);
                    vD *= width;
                    vN1.X = -vD.Y;
                    vN1.Y = vD.X;
                    vN2.X = vD.Y;
                    vN2.Y = -vD.X;
                }

                var color = PPDColors.White.ToBgra();
                dataVertices[drawVerticesCount].Position = new Vector3(pos[i].X + vN1.X, pos[i].Y + vN1.Y, 0.5f);
                dataVertices[drawVerticesCount].TextureCoordinates = trace.ImageResource.GetActualUV(new Vector2(alpha, 0)) + trace.ImageResource.HalfPixel;
                dataVertices[drawVerticesCount].Color = color;
                drawVerticesCount++;
                dataVertices[drawVerticesCount].Position = new Vector3(pos[i].X + vN2.X, pos[i].Y + vN2.Y, 0.5f);
                dataVertices[drawVerticesCount].TextureCoordinates = trace.ImageResource.GetActualUV(new Vector2(alpha, 1)) - new Vector2(-trace.ImageResource.HalfPixel.X, trace.ImageResource.HalfPixel.Y);
                dataVertices[drawVerticesCount].Color = color;
                drawVerticesCount++;
                alpha = 0.70f * (i + 1) / 40f;
            }
            vertices.Write(dataVertices, drawVerticesCount, 0);
            return drawVerticesCount;
        }

        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = trace.ImageResource.Texture;
            if (drawVerticesCount1 > 2)
            {
                alphaBlendContext.Vertex = vertices1;
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleStrip, drawVerticesCount1 - 2, 0, drawVerticesCount1);
            }
            if (drawVerticesCount2 > 2)
            {
                alphaBlendContext.Vertex = vertices2;
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleStrip, drawVerticesCount2 - 2, 0, drawVerticesCount2);
            }
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return DrawTrace;
        }

        protected override void DisposeResource()
        {
            if (vertices1 != null)
            {
                vertices1.Dispose();
                vertices1 = null;
            }
            if (vertices2 != null)
            {
                vertices2.Dispose();
                vertices2 = null;
            }
        }
    }
}