using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;
using System;

namespace PPDShareComponent
{
    public class LineComponent : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        Vector2[] points;
        float lineWidth;
        bool lineChanged;
        VertexInfo vertices;
        ColorTextureResourceBase resource;

        /// <summary>
        /// 色を取得、設定します。
        /// </summary>
        public Color4 Color
        {
            get
            {
                return resource.Color;
            }
            set
            {
                UpdateResource(value);
            }
        }

        public Vector2[] Points
        {
            get { return points; }
            set
            {
                if (points != value)
                {
                    points = value;
                    UpdateVertex();
                    lineChanged = true;
                }
            }
        }

        public float LineWidth
        {
            get { return lineWidth; }
            set
            {
                if (lineWidth != value)
                {
                    lineWidth = value;
                    lineChanged = true;
                }
            }
        }

        public LineComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Color4 color) : base(device)
        {
            this.resourceManager = resourceManager;
            UpdateResource(color);
        }

        private void UpdateResource(Color4 color)
        {
            var resourceName = String.Format("ColorTexture_{0}", color);
            resource = resourceManager.GetResource<ColorTextureResourceBase>(resourceName);
            if (resource == null)
            {
                resource = new ColorTextureResource(device, color);
                resourceManager.Add(resourceName, resource);
            }
        }

        private void UpdateVertex()
        {
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }

            if (points == null || points.Length < 2)
            {
                return;
            }

            vertices = device.GetModule<ShaderCommon>().CreateVertex(points.Length * 2);
        }

        private Vector2 Solve(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float ad_bc = 1 / (-b.X * d.Y + d.X * b.Y);
            if (float.IsNaN(ad_bc) || ad_bc >= 1000)
            {
                return a;
            }
            float c_a_x = c.X - a.X, c_a_y = c.Y - a.Y;
            float alpha = ad_bc * (-d.Y * c_a_x - b.Y * c_a_y);
            float beta = ad_bc * (d.X * c_a_x + b.X * c_a_y);
            return a + alpha * b;
        }

        protected override void UpdateImpl()
        {
            if (lineChanged)
            {
                lineChanged = false;
                var lineWidth2 = lineWidth / 2;
                if (points != null && points.Length >= 2 && vertices != null)
                {
                    ColoredTexturedVertex[] writeVertices = new ColoredTexturedVertex[points.Length * 2];
                    for (var i = 0; i < points.Length - 1; i++)
                    {
                        Vector2 nextDirection = (points[i + 1] - points[i]);
                        nextDirection.Normalize();
                        var nextDirectionNormal = new Vector2(-nextDirection.Y, nextDirection.X);
                        if (i == 0)
                        {
                            writeVertices[2 * i] = new ColoredTexturedVertex(new Vector3(points[i] + nextDirectionNormal * lineWidth2, 0.5f));
                            writeVertices[2 * i + 1] = new ColoredTexturedVertex(new Vector3(points[i] - nextDirectionNormal * lineWidth2, 0.5f));
                        }
                        else
                        {
                            var vec = new Vector2(writeVertices[2 * i].Position.X - points[i].X, writeVertices[2 * i].Position.Y - points[i].Y);
                            var normalized = Vector2.Normalize(vec);
                            var normal = new Vector2(-normalized.Y, normalized.X);
                            var newPos = Solve(vec, normal, nextDirectionNormal * lineWidth2, nextDirection);
                            writeVertices[2 * i] = new ColoredTexturedVertex(new Vector3(points[i] + newPos, 0.5f));
                            writeVertices[2 * i + 1] = new ColoredTexturedVertex(new Vector3(points[i] - newPos, 0.5f));
                        }
                        writeVertices[2 * i + 2] = new ColoredTexturedVertex(new Vector3(points[i + 1] + nextDirectionNormal * lineWidth2, 0.5f));
                        writeVertices[2 * i + 3] = new ColoredTexturedVertex(new Vector3(points[i + 1] - nextDirectionNormal * lineWidth2, 0.5f));
                    }
                    vertices.Write(writeVertices);
                }
            }
        }

        protected override void DrawImpl(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Vertex = vertices;
            alphaBlendContext.Texture = resource.Texture;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleStrip, (points.Length - 1) * 2, 0, vertices.Count);
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return vertices != null && resource != null;
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }
        }
    }
}
