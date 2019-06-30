using PPDFramework.Effect;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPDFramework.Sprites
{
    class SpriteBatch : DisposableComponent
    {
        ColoredTexturedVertex[] _vertices = new ColoredTexturedVertex[1024 * 32];
        int[] _indices = new int[1024 * 32 * 3];

        PPDDevice device;
        VertexBufferBase vertices;
        IndexBufferBase indices;
        List<SpriteInfo> spriteInfos;
        List<ScissorRectState> scissorRectStates;
        Effect.EffectBase effect;
        Effect.EffectHandleBase textureHandle;
        Effect.EffectHandleBase overlayColorHandle;
        Effect.EffectHandleBase projectionHandle;
        Dictionary<bool, string> techniques;

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
            spriteInfos = new List<SpriteInfo>();
            scissorRectStates = new List<ScissorRectState>();
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
            batches.Add(new BatchInfo(spriteInfos[spriteInfos.Count - 1], vertexPos, vertexEndPos - vertexPos, indexPos, indexEndPos - indexPos, lastScissorRectIndex));
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
            var _device = (SharpDX.Direct3D9.Device)((DX9.PPDDevice)device).Device;
            _device.ScissorRect = scissorRectStates[scissorRectIndex].Rectangle;
            _device.SetRenderState(RenderState.ScissorTestEnable, scissorRectStates[scissorRectIndex].Enabled);
        }

        private void PrepareBatchDraw(int vertexPos, int indexPos, bool pa)
        {
            vertices.Write(_vertices, vertexPos, 0);
            indices.Write(_indices, indexPos, 0);
            var _device = (device.Device as SharpDX.Direct3D9.Device);
            _device.SetRenderState(SharpDX.Direct3D9.RenderState.AlphaBlendEnable, true);
            ChangePAMode(pa);
            _device.SetRenderState(SharpDX.Direct3D9.RenderState.DestinationBlend, SharpDX.Direct3D9.Blend.InverseSourceAlpha);
            device.SetStreamSource(vertices);
            device.SetIndices(indices);
            device.VertexDeclaration = effect.VertexDeclaration;
        }

        private void ChangePAMode(bool pa)
        {
            var _device = (device.Device as SharpDX.Direct3D9.Device);
            _device.SetRenderState(SharpDX.Direct3D9.RenderState.SourceBlend, pa ? SharpDX.Direct3D9.Blend.One : SharpDX.Direct3D9.Blend.SourceAlpha);
            effect.Technique = techniques[pa];
        }

        private void DrawBatch(BatchInfo batchInfo)
        {
            effect.SetTexture(textureHandle, batchInfo.SpriteInfo.Texture);
            effect.SetValue(overlayColorHandle, batchInfo.SpriteInfo.OverlayColor);
            var passCount = effect.Begin();
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
            if (textureHandle != null)
            {
                textureHandle.Dispose();
                textureHandle = null;
            }
            if (overlayColorHandle != null)
            {
                overlayColorHandle.Dispose();
                overlayColorHandle = null;
            }
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }

        class SpriteInfo
        {
            public Texture.TextureBase Texture { get; private set; }
            public Matrix Matrix { get; private set; }
            public float Alpha { get; private set; }
            public ColoredTexturedVertex[] Vertices { get; private set; }
            public Color4 OverlayColor { get; private set; }
            public PrimitiveType PrimitiveType { get; private set; }
            public int Offset { get; private set; }
            public int Count { get; private set; }
            public int ScissorRectIndex { get; private set; }

            public SpriteInfo(Texture.TextureBase texture, Matrix matrix, float alpha, Color4 overlayColor, PrimitiveType primitiveType, ColoredTexturedVertex[] vertices, int offset, int count, int scissorRectIndex)
            {
                Texture = texture;
                Matrix = matrix;
                Alpha = alpha;
                Vertices = vertices;
                OverlayColor = overlayColor;
                PrimitiveType = primitiveType;
                Offset = offset;
                Count = count;
                ScissorRectIndex = scissorRectIndex;
            }

            public void Transform()
            {
                var temp = new ColoredTexturedVertex[Count];
                for (var i = 0; i < Count; i++)
                {
                    temp[i] = Vertices[i + Offset];
                    temp[i].Position = Vector3.TransformCoordinate(temp[i].Position, Matrix);
                    var newColor = new ColorBGRA(temp[i].Color);
                    newColor.A = (byte)(newColor.A * Alpha);
                    temp[i].Color = newColor.ToBgra();
                }
                Vertices = temp;
            }
        }

        class BatchInfo
        {
            public SpriteInfo SpriteInfo { get; private set; }
            public int VertexPos { get; private set; }
            public int IndexPos { get; private set; }
            public int VertexCount { get; private set; }
            public int IndexCount { get; private set; }
            public int ScissorRectIndex { get; private set; }
            public bool PA { get { return SpriteInfo.Texture.PA; } }

            public BatchInfo(SpriteInfo spriteInfo, int vertexPos, int vertexCount, int indexPos, int indexCount, int scissorRectIndex)
            {
                SpriteInfo = spriteInfo;
                VertexPos = vertexPos;
                VertexCount = vertexCount;
                IndexPos = indexPos;
                IndexCount = indexCount;
                ScissorRectIndex = scissorRectIndex;
            }
        }

        class ScissorRectState
        {
            public Rectangle Rectangle
            {
                get;
                private set;
            }

            public bool Enabled
            {
                get;
                private set;
            }

            public ScissorRectState(Rectangle rectangle, bool enabled)
            {
                Rectangle = rectangle;
                Enabled = enabled;
            }
        }
    }
}
