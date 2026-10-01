using PPDFramework.Effect;
using PPDFramework.Vertex;
using System.Numerics;
using PPDMath;
using Matrix = System.Numerics.Matrix4x4;
using Color4 = Vortice.Mathematics.Color4;
using Rectangle = Vortice.Mathematics.RectI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPDFramework.Sprites
{
    class SpriteBatch : DisposableComponent
    {
        readonly ColoredTexturedVertex[] _vertices = new ColoredTexturedVertex[1024 * 32];
        readonly int[] _indices = new int[1024 * 32 * 3];

        readonly PPDDevice device;
        VertexBufferBase vertices;
        IndexBufferBase indices;
        readonly List<SpriteInfo> spriteInfos;
        readonly List<ScissorRectState> scissorRectStates;
        Effect.EffectBase effect;
        Effect.EffectHandleBase textureHandle;
        Effect.EffectHandleBase overlayColorHandle;
        Effect.EffectHandleBase projectionHandle;
        readonly Dictionary<bool, string> techniques;

        public SpriteBatch(PPDDevice device)
        {
            this.device = device;
            effect = EffectFactoryManager.Factory.FromMemory(device, device.GetShaderBytecode("BasicEffect"));
            textureHandle = effect.GetParameter("Texture");
            overlayColorHandle = effect.GetParameter("OverlayColor");
            projectionHandle = effect.GetParameter("Projection");
            effect.SetValue(projectionHandle, device.Projection);

            vertices = Vertex.VertexBufferFactoryManager.Factory.Create(device, _vertices.Length * ColoredTexturedVertex.Size);
            indices = Vertex.IndexBufferFactoryManager.Factory.Create(device, _indices.Length * sizeof(int));
            spriteInfos = [];
            scissorRectStates = [];
            techniques = new Dictionary<bool, string>
            {
                {false, "BasicEffectPADisabled"},
                {true, "BasicEffectPAEnabled"}
            };
        }

        public void Draw(Texture.TextureBase texture, Matrix matrix, float alpha, Color4 overlayColor, PrimitiveType primitiveType, ColoredTexturedVertex[] vertices, int offset, int count)
        {
            if (texture == null)
            {
                return;
            }
            spriteInfos.Add(new SpriteInfo(texture, matrix, alpha, overlayColor, primitiveType, vertices, offset, count, scissorRectStates.Count - 1));
        }

        public void SetScissorRect(Rectangle rectangle, bool enabled)
        {
            scissorRectStates.Add(new ScissorRectState(rectangle, enabled));
        }

        public void Flush()
        {
            if (spriteInfos.Count == 0)
            {
                return;
            }
#if BENCHMARK
            using (var handler = Benchmark.Instance.Start("Sprite Batch Flush"))
            {
#endif
                FlushImpl();
#if BENCHMARK
            }
#endif
        }

        private void FlushImpl()
        {
            Parallel.ForEach(spriteInfos, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, spriteInfo => spriteInfo.Transform());
            var batches = new List<BatchInfo>();
            var vertexPos = 0;
            var vertexEndPos = 0;
            var indexPos = 0;
            var indexEndPos = 0;
            var spriteIndex = 0;
            var lastScissorRectIndex = spriteInfos[0].ScissorRectIndex;
            for (var i = 1; i < spriteInfos.Count; i++)
            {
                var prev = spriteInfos[i - 1];
                var current = spriteInfos[i];
                if (prev.Texture != current.Texture || prev.Texture.PA != current.Texture.PA ||
                    prev.OverlayColor != current.OverlayColor || lastScissorRectIndex != current.ScissorRectIndex)
                {
                    WriteToVertex(spriteIndex, i - spriteIndex, ref vertexEndPos, ref indexEndPos);
                    batches.Add(new BatchInfo(prev, vertexPos, vertexEndPos - vertexPos, indexPos, indexEndPos - indexPos, lastScissorRectIndex));
                    vertexPos = vertexEndPos;
                    indexPos = indexEndPos;
                    spriteIndex = i;
                    lastScissorRectIndex = current.ScissorRectIndex;
                }
            }
            WriteToVertex(spriteIndex, spriteInfos.Count - spriteIndex, ref vertexEndPos, ref indexEndPos);
            batches.Add(new BatchInfo(spriteInfos[^1], vertexPos, vertexEndPos - vertexPos, indexPos, indexEndPos - indexPos, lastScissorRectIndex));
            PrepareBatchDraw(vertexEndPos, indexEndPos, batches[0].PA);
            SetScissorImpl(0);
            lastScissorRectIndex = 0;
            var lastPA = batches[0].PA;
            foreach (var batch in batches)
            {
                if (batch.ScissorRectIndex != lastScissorRectIndex)
                {
                    SetScissorImpl(batch.ScissorRectIndex);
                    lastScissorRectIndex = batch.ScissorRectIndex;
                }
                if (lastPA != batch.PA)
                {
                    ChangePAMode(batch.PA);
                    lastPA = batch.PA;
                }
                DrawBatch(batch);
            }

            spriteInfos.Clear();
        }

        public void End()
        {
            spriteInfos.Clear();
            scissorRectStates.Clear();
        }

        private void SetScissorImpl(int scissorRectIndex)
        {
            device.ApplyScissorRect(scissorRectStates[scissorRectIndex].Rectangle, scissorRectStates[scissorRectIndex].Enabled);
        }

        private void PrepareBatchDraw(int vertexPos, int indexPos, bool pa)
        {
            vertices.Write(_vertices, vertexPos, 0);
            indices.Write(_indices, indexPos, 0);
            ChangePAMode(pa);
            device.SetStreamSource(vertices);
            device.SetIndices(indices);
            device.VertexDeclaration = effect.VertexDeclaration;
        }

        private void ChangePAMode(bool pa)
        {
            device.SetSpriteAlphaBlend(pa);
            effect.Technique = techniques[pa];
        }

        private void DrawBatch(BatchInfo batchInfo)
        {
            effect.SetTexture(textureHandle, batchInfo.SpriteInfo.Texture);
            effect.SetValue(overlayColorHandle, batchInfo.SpriteInfo.OverlayColor);
            _ = effect.Begin();
            effect.BeginPass(0);
            effect.CommitChanges();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, batchInfo.VertexPos, batchInfo.VertexCount, batchInfo.IndexPos, batchInfo.IndexCount / 3);
            effect.EndPass();
            effect.End();
            effect.SetTexture(textureHandle, null);
        }

        private void WriteToVertex(int spriteOffset, int spriteCount, ref int vertexPos, ref int indexPos)
        {
            for (var i = 0; i < spriteCount; i++)
            {
                var spriteInfo = spriteInfos[spriteOffset + i];
                Array.Copy(spriteInfo.Vertices, 0, _vertices, vertexPos, spriteInfo.Vertices.Length);
                switch (spriteInfo.PrimitiveType)
                {
                    case PrimitiveType.TriangleFan:
                        for (var j = 0; j < spriteInfo.Vertices.Length - 2; j++)
                        {
                            var vertexOffset = vertexPos + j;
                            _indices[indexPos++] = vertexPos;
                            _indices[indexPos++] = vertexOffset + 1;
                            _indices[indexPos++] = vertexOffset + 2;
                        }
                        break;
                    case PrimitiveType.TriangleList:
                        var maxCount = spriteInfo.Vertices.Length / 3;
                        for (var j = 0; j < maxCount; j++)
                        {
                            var vertexOffset = vertexPos + 3 * j;
                            _indices[indexPos++] = vertexOffset;
                            _indices[indexPos++] = vertexOffset + 1;
                            _indices[indexPos++] = vertexOffset + 2;
                        }
                        break;
                    case PrimitiveType.TriangleStrip:
                        for (var j = 0; j < spriteInfo.Vertices.Length - 2; j++)
                        {
                            var vertexOffset = vertexPos + j;
                            _indices[indexPos++] = vertexOffset;
                            _indices[indexPos++] = vertexOffset + 1;
                            _indices[indexPos++] = vertexOffset + 2;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
                vertexPos += spriteInfo.Vertices.Length;
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            textureHandle?.Dispose();
            textureHandle = null;
            overlayColorHandle?.Dispose();
            overlayColorHandle = null;
            projectionHandle?.Dispose();
            projectionHandle = null;
            effect?.Dispose();
            effect = null;
            vertices?.Dispose();
            vertices = null;
            indices?.Dispose();
            indices = null;
        }

        class SpriteInfo(Texture.TextureBase texture, Matrix matrix, float alpha, Color4 overlayColor, PrimitiveType primitiveType, ColoredTexturedVertex[] vertices, int offset, int count, int scissorRectIndex)
        {
            public Texture.TextureBase Texture { get; private set; } = texture;
            public Matrix Matrix { get; private set; } = matrix;
            public float Alpha { get; private set; } = alpha;
            public ColoredTexturedVertex[] Vertices { get; private set; } = vertices;
            public Color4 OverlayColor { get; private set; } = overlayColor;
            public PrimitiveType PrimitiveType { get; private set; } = primitiveType;
            public int Offset { get; private set; } = offset;
            public int Count { get; private set; } = count;
            public int ScissorRectIndex { get; private set; } = scissorRectIndex;

            public void Transform()
            {
                var temp = new ColoredTexturedVertex[Count];
                for (var i = 0; i < Count; i++)
                {
                    temp[i] = Vertices[i + Offset];
                    temp[i].Position = Vector3.TransformCoordinate(temp[i].Position, Matrix);
                    // BGRA の A だけに Alpha を掛ける（SharpDX の ColorBGRA を使っていたときと同じ計算）
                    var alpha = (byte)((byte)((temp[i].Color >> 24) & 0xFF) * Alpha);
                    temp[i].Color = (temp[i].Color & 0x00FFFFFF) | (alpha << 24);
                }
                Vertices = temp;
            }
        }

        class BatchInfo(SpriteBatch.SpriteInfo spriteInfo, int vertexPos, int vertexCount, int indexPos, int indexCount, int scissorRectIndex)
        {
            public SpriteInfo SpriteInfo { get; private set; } = spriteInfo;
            public int VertexPos { get; private set; } = vertexPos;
            public int IndexPos { get; private set; } = indexPos;
            public int VertexCount { get; private set; } = vertexCount;
            public int IndexCount { get; private set; } = indexCount;
            public int ScissorRectIndex { get; private set; } = scissorRectIndex;
            public bool PA { get { return SpriteInfo.Texture.PA; } }
        }

        class ScissorRectState(Rectangle rectangle, bool enabled)
        {
            public Rectangle Rectangle
            {
                get;
                private set;
            } = rectangle;

            public bool Enabled
            {
                get;
                private set;
            } = enabled;
        }
    }
}
