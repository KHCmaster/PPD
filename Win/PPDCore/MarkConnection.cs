using PPDFramework;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    class MarkConnection : GameComponent
    {
        List<Mark> convexMarks;
        List<Mark> inMarks;
        MarkConnectionCommon connectionCommon;

        public MarkConnection(PPDDevice device, MarkConnectionCommon connectionCommon) : base(device)
        {
            this.connectionCommon = connectionCommon;
            convexMarks = new List<Mark>();
            inMarks = new List<Mark>();
        }

        public void AddConvex(Mark mk)
        {
            convexMarks.Add(mk);
        }

        public void AddIn(Mark mk)
        {
            inMarks.Add(mk);
        }

        public void Initialize()
        {
            Mark lastmk = convexMarks[0];
            for (int i = 1; i < convexMarks.Count; i++)
            {
                if (lastmk.DoNotDrawConnect || convexMarks[i].DoNotDrawConnect)
                {
                    continue;
                }
                this.AddChild(new MarkConnectionLine(device, lastmk, convexMarks[i], connectionCommon));
                lastmk = convexMarks[i];
            }
            if (convexMarks.Count >= 3)
            {
                if (!lastmk.DoNotDrawConnect && !convexMarks[0].DoNotDrawConnect)
                {
                    this.AddChild(new MarkConnectionLine(device, lastmk, convexMarks[0], connectionCommon));
                }
            }
            foreach (Mark inmk in inMarks)
            {
                foreach (Mark convmk in convexMarks)
                {
                    if (inmk.DoNotDrawConnect || convmk.DoNotDrawConnect)
                    {
                        continue;
                    }
                    this.AddChild(new MarkConnectionLine(device, inmk, convmk, connectionCommon));
                }
            }
        }

        public bool Contains(Mark mk)
        {
            return convexMarks.Contains(mk) || inMarks.Contains(mk);
        }

        class MarkConnectionLine : GameComponent
        {
            MarkConnectionCommon markConnectionCommon;
            Mark mk1;
            Mark mk2;
            int drawCount;
            VertexInfo[] vertices;

            public MarkConnectionLine(PPDDevice device, Mark mk1, Mark mk2, MarkConnectionCommon markConnectionCommon) : base(device)
            {
                this.markConnectionCommon = markConnectionCommon;
                this.mk1 = mk1;
                this.mk2 = mk2;
                vertices = new VertexInfo[MarkConnectionCommon.ConnectionCount];
                for (int i = 0; i < MarkConnectionCommon.ConnectionCount; i++)
                {
                    vertices[i] = device.GetModule<ShaderCommon>().CreateVertex(MarkConnectionCommon.SplitCount);
                }
            }

            protected override void UpdateImpl()
            {
                var pos1 = mk1.ColorPosition + mk1.Position;
                var pos2 = mk2.ColorPosition + mk2.Position;
                var vec = Vector2.Subtract(pos1, pos2);
                if (vec == Vector2.Zero)
                {
                    return;
                }
                vec.Normalize();
                Position = (pos1 + pos2) / 2;
                if (vec.Y < 0)
                {
                    Rotation = (float)(Math.PI * 2 - (float)Math.Acos(vec.X));
                }
                else
                {
                    Rotation = (float)Math.Acos(vec.X);
                }
                var width = Vector2.Distance(pos1, pos2);
                drawCount = (int)Math.Ceiling(width / MarkConnectionCommon.WidthPerCount) * 2 + 2;
                drawCount = drawCount < 4 ? 4 : drawCount;
                drawCount = drawCount > MarkConnectionCommon.SplitCount ? MarkConnectionCommon.SplitCount : drawCount;
                Scale = new Vector2(width / (drawCount * MarkConnectionCommon.WidthPerCount / 2), 1);
                var rect = markConnectionCommon.ActualUVRectangle;
                for (var i = 0; i < markConnectionCommon.Connects.Length; i++)
                {
                    var vertexs = markConnectionCommon.Connects[i];
                    for (int j = 0; j < vertexs.Length; j += 2)
                    {
                        float alpha = j >= (MarkConnectionCommon.SplitCount - drawCount) / 2 && j <= (MarkConnectionCommon.SplitCount + drawCount) / 2 ?
                            1f - Math.Abs(MarkConnectionCommon.SplitCount / 2 - j) / ((float)drawCount / 2) : 0;
                        alpha *= 0.99f;
                        var alphaWidth = new Vector2(rect.Width * alpha, 0);
                        vertexs[j].TextureCoordinates = rect.TopLeft + alphaWidth;
                        vertexs[j + 1].TextureCoordinates = rect.BottomLeft + alphaWidth;
                    }
                    vertices[i].Write(vertexs);
                }
            }

            protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
            {
                alphaBlendContext.Texture = markConnectionCommon.ImageResource.Texture;
                foreach (var vertex in vertices)
                {
                    alphaBlendContext.Vertex = vertex;
                    device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleStrip, drawCount - 2,
                        (MarkConnectionCommon.SplitCount - drawCount) / 2, drawCount);
                }
            }

            protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
            {
                return !(mk1.Hidden || mk2.Hidden || mk1.ColorHidden || mk2.ColorHidden);
            }

            protected override void DisposeResource()
            {
                if (vertices != null)
                {
                    foreach (var vertex in vertices)
                    {
                        vertex.Dispose();
                    }
                    vertices = null;
                }
            }
        }
    }
}
