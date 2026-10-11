using PPDFramework.Effect;
using PPDFramework.Vertex;
using System.Numerics;
using PPDMath;
using Matrix = System.Numerics.Matrix4x4;
using Color4 = Vortice.Mathematics.Color4;
using Rectangle = Vortice.Mathematics.RectI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PPDFramework.Sprites
{
    class SpriteBatch : DisposableComponent
    {
        // スプライトがこの数以上のときだけ、頂点の変換を並列にする（少ないときは並列化のコストのほうが大きい）
        const int ParallelTransformThreshold = 1024;

        readonly ColoredTexturedVertex[] _vertices = new ColoredTexturedVertex[1024 * 32];
        readonly int[] _indices = new int[1024 * 32 * 3];

        readonly PPDDevice device;
        VertexBufferBase vertices;
        IndexBufferBase indices;
        // 毎フレームの確保を避けるため、スプライトとバッチの情報は構造体にして、リストを使い回す
        readonly List<SpriteInfo> spriteInfos;
        readonly List<BatchInfo> batches = [];
        readonly Action<int> transformAt;
        int spriteVertexCount;
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
            transformAt = TransformAt;
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
            // 頂点は Flush のときに読む（それまでに呼び出し側が配列を書き換えても、Flush のときの内容を使う）
            spriteInfos.Add(new SpriteInfo(texture, matrix, alpha, overlayColor, primitiveType, vertices, offset, count, scissorRectStates.Count - 1, spriteVertexCount));
            spriteVertexCount += count;
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
            if (spriteInfos.Count >= ParallelTransformThreshold)
            {
                Parallel.For(0, spriteInfos.Count, transformAt);
            }
            else
            {
                for (var i = 0; i < spriteInfos.Count; i++)
                {
                    TransformAt(i);
                }
            }
            batches.Clear();
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
                    WriteIndices(spriteIndex, i - spriteIndex, ref vertexEndPos, ref indexEndPos);
                    batches.Add(new BatchInfo(prev.Texture, prev.OverlayColor, vertexPos, vertexEndPos - vertexPos, indexPos, indexEndPos - indexPos, lastScissorRectIndex));
                    vertexPos = vertexEndPos;
                    indexPos = indexEndPos;
                    spriteIndex = i;
                    lastScissorRectIndex = current.ScissorRectIndex;
                }
            }
            WriteIndices(spriteIndex, spriteInfos.Count - spriteIndex, ref vertexEndPos, ref indexEndPos);
            var last = spriteInfos[^1];
            batches.Add(new BatchInfo(last.Texture, last.OverlayColor, vertexPos, vertexEndPos - vertexPos, indexPos, indexEndPos - indexPos, lastScissorRectIndex));
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
            batches.Clear();
            spriteVertexCount = 0;
        }

        public void End()
        {
            spriteInfos.Clear();
            spriteVertexCount = 0;
            scissorRectStates.Clear();
        }

        /// <summary>
        /// スプライトの頂点を変換して、共有の頂点配列の決まった位置に書き込みます（スプライトごとに範囲が重ならないので並列に呼べる）。
        /// </summary>
        private void TransformAt(int index)
        {
            ref readonly var spriteInfo = ref CollectionsMarshal.AsSpan(spriteInfos)[index];
            var source = spriteInfo.Vertices;
            for (var i = 0; i < spriteInfo.Count; i++)
            {
                var vertex = source[i + spriteInfo.Offset];
                vertex.Position = Vector3.TransformCoordinate(vertex.Position, spriteInfo.Matrix);
                // BGRA の A だけに Alpha を掛ける（SharpDX の ColorBGRA を使っていたときと同じ計算）
                var alpha = (byte)((byte)((vertex.Color >> 24) & 0xFF) * spriteInfo.Alpha);
                vertex.Color = (vertex.Color & 0x00FFFFFF) | (alpha << 24);
                _vertices[spriteInfo.VertexStart + i] = vertex;
            }
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

        private void DrawBatch(in BatchInfo batchInfo)
        {
            effect.SetTexture(textureHandle, batchInfo.Texture);
            effect.SetValue(overlayColorHandle, batchInfo.OverlayColor);
            _ = effect.Begin();
            effect.BeginPass(0);
            effect.CommitChanges();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, batchInfo.VertexPos, batchInfo.VertexCount, batchInfo.IndexPos, batchInfo.IndexCount / 3);
            effect.EndPass();
            effect.End();
            effect.SetTexture(textureHandle, null);
        }

        /// <summary>
        /// インデックスを書き込みます。頂点は TransformAt で書き込み済み（スプライトの順に並んでいる）。
        /// </summary>
        private void WriteIndices(int spriteOffset, int spriteCount, ref int vertexPos, ref int indexPos)
        {
            for (var i = 0; i < spriteCount; i++)
            {
                ref readonly var spriteInfo = ref CollectionsMarshal.AsSpan(spriteInfos)[spriteOffset + i];
                var vertexCount = spriteInfo.Count;
                switch (spriteInfo.PrimitiveType)
                {
                    case PrimitiveType.TriangleFan:
                        for (var j = 0; j < vertexCount - 2; j++)
                        {
                            var vertexOffset = vertexPos + j;
                            _indices[indexPos++] = vertexPos;
                            _indices[indexPos++] = vertexOffset + 1;
                            _indices[indexPos++] = vertexOffset + 2;
                        }
                        break;
                    case PrimitiveType.TriangleList:
                        var maxCount = vertexCount / 3;
                        for (var j = 0; j < maxCount; j++)
                        {
                            var vertexOffset = vertexPos + 3 * j;
                            _indices[indexPos++] = vertexOffset;
                            _indices[indexPos++] = vertexOffset + 1;
                            _indices[indexPos++] = vertexOffset + 2;
                        }
                        break;
                    case PrimitiveType.TriangleStrip:
                        for (var j = 0; j < vertexCount - 2; j++)
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
                vertexPos += vertexCount;
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

        readonly struct SpriteInfo(Texture.TextureBase texture, Matrix matrix, float alpha, Color4 overlayColor, PrimitiveType primitiveType, ColoredTexturedVertex[] vertices, int offset, int count, int scissorRectIndex, int vertexStart)
        {
            public readonly Texture.TextureBase Texture = texture;
            public readonly Matrix Matrix = matrix;
            public readonly float Alpha = alpha;
            public readonly ColoredTexturedVertex[] Vertices = vertices;
            public readonly Color4 OverlayColor = overlayColor;
            public readonly PrimitiveType PrimitiveType = primitiveType;
            public readonly int Offset = offset;
            public readonly int Count = count;
            public readonly int ScissorRectIndex = scissorRectIndex;
            // 変換後の頂点を書き込む、共有の頂点配列の位置
            public readonly int VertexStart = vertexStart;
        }

        readonly struct BatchInfo(Texture.TextureBase texture, Color4 overlayColor, int vertexPos, int vertexCount, int indexPos, int indexCount, int scissorRectIndex)
        {
            public readonly Texture.TextureBase Texture = texture;
            public readonly Color4 OverlayColor = overlayColor;
            public readonly int VertexPos = vertexPos;
            public readonly int IndexPos = indexPos;
            public readonly int VertexCount = vertexCount;
            public readonly int IndexCount = indexCount;
            public readonly int ScissorRectIndex = scissorRectIndex;
            public bool PA { get { return Texture.PA; } }
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
