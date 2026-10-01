using Effect2D;
using PPDFramework.Effect;
using PPDFramework.Vertex;
using System.Numerics;
using PPDMath;
using Matrix = System.Numerics.Matrix4x4;
using System;
using System.Collections.Generic;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// アルファブレンドのクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    public class AlphaBlend(PPDDevice device) : DisposableComponent
    {
        AlphaBlendBase alphaBlend = new AlphaBlendShading(device);

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context"></param>
        /// <param name="primitiveType"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="startIndex"></param>
        /// <param name="vertexCount"></param>
        public void Draw(PPDDevice device, AlphaBlendContext context, PrimitiveType primitiveType = PrimitiveType.TriangleStrip,
            int primitiveCount = 2, int startIndex = 0, int vertexCount = 4)
        {
            var cloneContext = device.GetModule<AlphaBlendContextCache>().Clone(context);
            DrawInternal(device, cloneContext, primitiveType, primitiveCount, startIndex, vertexCount);
        }

        internal void DrawInternal(PPDDevice device, AlphaBlendContext context, PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount)
        {
            alphaBlend.Draw(device, context, primitiveType, primitiveCount, startIndex, vertexCount);
        }

        /// <summary>
        /// フラッシュします。
        /// </summary>
        public void Flush()
        {
            alphaBlend.Flush();
        }

        /// <summary>
        /// マスクを開始します。
        /// </summary>
        /// <param name="maskMode"></param>
        /// <returns></returns>
        public IDisposable StartMaskGeneration(MaskType maskMode)
        {
            return alphaBlend.StartMaskGeneration(maskMode);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            alphaBlend?.Dispose();
            alphaBlend = null;
        }

        abstract class AlphaBlendBase : DisposableComponent
        {
            public bool IsMasking
            {
                get { return maskHandlers.Count > 0; }
            }

            public MaskType MaskType
            {
                get { return maskHandlers.Peek().MaskType; }
            }

            readonly Stack<MaskHandler> maskHandlers;

            protected AlphaBlendBase()
            {
                maskHandlers = new Stack<MaskHandler>();
            }

            public abstract void Draw(PPDDevice device, AlphaBlendContext context, PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount);

            public virtual void Flush()
            {

            }

            public IDisposable StartMaskGeneration(MaskType maskMode)
            {
                var handler = new MaskHandler(this, maskMode);
                maskHandlers.Push(handler);
                return handler;
            }

            private void EndMaskGeneration(MaskHandler handler)
            {
                maskHandlers.Pop();
            }

            class MaskHandler(AlphaBlend.AlphaBlendBase alphaBlend, MaskType maskType) : DisposableComponent
            {
                readonly AlphaBlendBase alphaBlend = alphaBlend;

                public MaskType MaskType
                {
                    get;
                    private set;
                } = maskType;

                protected override void DisposeResource()
                {
                    alphaBlend.EndMaskGeneration(this);
                }
            }
        }

        class AlphaBlendShading : AlphaBlendBase
        {
            readonly PPDDevice device;
            EffectBase effect;
            EffectHandleBase lastRenderTargetTextureHandle;
            EffectHandleBase textureHandle;
            EffectHandleBase maskTextureHandle;
            EffectHandleBase drawInfoHandle;
            EffectHandleBase widthHeightHandle;
            EffectHandleBase projectionHandle;
            EffectHandleBase filterInfoHandle;
            readonly Dictionary<Tuple<BlendMode, bool, bool, ColorFilterType>, string> techniqueNames;
            readonly Dictionary<Tuple<MaskType, bool>, string> maskTechniqueNames;

            public AlphaBlendShading(PPDDevice device)
            {
                this.device = device;
                effect = EffectFactoryManager.Factory.FromMemory(device, device.GetShaderBytecode("AlphaBlend"));
                textureHandle = effect.GetParameter("Texture");
                lastRenderTargetTextureHandle = effect.GetParameter("LastRenderTargetTexture");
                maskTextureHandle = effect.GetParameter("MaskTexture");
                drawInfoHandle = effect.GetParameter("DrawInfo");
                widthHeightHandle = effect.GetParameter("WidthHeight");
                projectionHandle = effect.GetParameter("Projection");
                filterInfoHandle = effect.GetParameter("FilterInfo");
                effect.SetValue(widthHeightHandle, new Vector2(device.Width, device.Height));
                effect.SetValue(projectionHandle, device.Projection);
                techniqueNames = [];
                foreach (var blendMode in (BlendMode[])Enum.GetValues(typeof(BlendMode)))
                {
                    foreach (var isPA in new bool[] { true, false })
                    {
                        foreach (var isMasking in new bool[] { true, false })
                        {
                            foreach (var filterType in (ColorFilterType[])Enum.GetValues(typeof(ColorFilterType)))
                            {
                                techniqueNames[new Tuple<BlendMode, bool, bool, ColorFilterType>(blendMode, isPA, isMasking, filterType)] =
                                    String.Format("{0}Blend{1}{2}{3}Filter",
                                        blendMode,
                                        isPA ? "PAEnabled" : "PADisabled",
                                        isMasking ? "MaskEnabled" : "MaskDisabled",
                                        filterType);
                            }
                        }

                    }
                }
                maskTechniqueNames = [];
                foreach (var maskType in (MaskType[])Enum.GetValues(typeof(MaskType)))
                {
                    foreach (var isMasking in new bool[] { true, false })
                    {
                        maskTechniqueNames[new Tuple<MaskType, bool>(maskType, isMasking)] =
                            String.Format("{0}Mask{1}",
                                maskType,
                                isMasking ? "MaskEnabled" : "MaskDisabled");
                    }
                }
            }

            public override void Draw(PPDDevice device, AlphaBlendContext context, PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount)
            {
                Matrix m = context.SRTS[0];
                if (!context.WorldDisabled)
                {
                    m *= device.World;
                }
                for (var i = 1; i <= context.SRTDepth; i++)
                {
                    m = context.SRTS[i] * m;
                }
                if (IsMasking)
                {
                    DrawMask(device, context, m, primitiveType, primitiveCount, startIndex, vertexCount);
                }
                else
                {
                    if (context.BlendMode == BlendMode.Glass)
                    {
                        DrawGlass(device, context, m, primitiveType, primitiveCount, startIndex, vertexCount);
                    }
                    else
                    {
                        if (context.BlendMode == BlendMode.Normal && !context.Transparent && context.FilterCount == 0 && context.MaskTexture == null)
                        {
                            DrawNormalBlend(device, context, m, primitiveType, primitiveCount, startIndex, vertexCount);
                        }
                        else
                        {
                            using var workspaceTexture = device.Workspace.Get();
                            Flush();
#if BENCHMARK
                                using (var handler = Benchmark.Instance.Start("Copy"))
                                {
#endif
                            device.StretchRectangle(device.GetRenderTarget(), workspaceTexture);
#if BENCHMARK
                                }
#endif
                            DrawNotNormalBlend(device, context, m, workspaceTexture, primitiveType, primitiveCount, startIndex, vertexCount);
                        }
                    }
                }
            }

            public override void Flush()
            {
                device.SpriteBatch.Flush();
            }

            private void DrawMask(PPDDevice device, AlphaBlendContext context, Matrix m, PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount)
            {
                using var workspaceTexture = device.Workspace.Get();
#if BENCHMARK
                    using (var handler = Benchmark.Instance.Start("Copy"))
                    {
#endif
                device.StretchRectangle(device.GetRenderTarget(), workspaceTexture);
#if BENCHMARK
                    }
#endif
#if BENCHMARK
                    using (var handler = Benchmark.Instance.Start("Effect Prepare"))
                    {
#endif
                effect.Technique = maskTechniqueNames[new Tuple<MaskType, bool>(MaskType, context.MaskTexture != null)];
                effect.SetTexture(textureHandle, context.Texture);
                effect.SetTexture(lastRenderTargetTextureHandle, workspaceTexture.Texture);
                effect.SetTexture(maskTextureHandle, context.MaskTexture);
                effect.SetValue(drawInfoHandle, new DrawInfo
                {
                    Matrix = m,
                    Alpha = context.Alpha,
                    OverlayColor = context.Overlay
                });
#if BENCHMARK
                    }
#endif

#if BENCHMARK
                    using (var handler = Benchmark.Instance.Start("Mask Draw"))
                    {
#endif
                var passCount = effect.Begin();
                effect.BeginPass(0);
                effect.CommitChanges();
                device.VertexDeclaration = effect.VertexDeclaration;
                device.SetStreamSource(context.Vertex.VertexBucket.VertexBuffer);
                device.DrawPrimitives(primitiveType, context.Vertex.Offset + startIndex, primitiveCount);
                effect.EndPass();
                effect.End();
                effect.SetTexture(textureHandle, null);
#if BENCHMARK
                    }
#endif
            }

            private void DrawGlass(PPDDevice device, AlphaBlendContext context, Matrix m,
                PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount)
            {
                using var workspaceTexture = device.Workspace.Get();
                Flush();
#if BENCHMARK
                    using (var handler = Benchmark.Instance.Start("Copy"))
                    {
#endif
                device.StretchRectangle(device.GetRenderTarget(), workspaceTexture);
#if BENCHMARK
                    }
#endif
                using var tempWorkspaceTexture = device.Workspace.Get();
                using var tempWorkspaceTexture2 = device.Workspace.Get();
                context.BlendMode = BlendMode.Normal;
                var renderTarget = device.GetRenderTarget();
                device.SetRenderTarget(tempWorkspaceTexture);
                device.Clear();
                device.SetRenderTarget(tempWorkspaceTexture2);
                device.Clear();
                device.SetRenderTarget(tempWorkspaceTexture);
                DrawNotNormalBlend(device, context, m, tempWorkspaceTexture2, primitiveType, primitiveCount, startIndex, vertexCount);
                device.SetRenderTarget(renderTarget);
                device.GetModule<GlassFilter>().Draw(device, tempWorkspaceTexture.Texture, workspaceTexture.Texture);
            }

            private void DrawNormalBlend(PPDDevice device, AlphaBlendContext context, Matrix m,
                PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount)
            {
#if BENCHMARK
                using (var handler = Benchmark.Instance.Start("Sprite Batch Draw"))
                {

#endif
                    if (context?.Vertex?.VertexBucket?.VertexBuffer is VertexBufferBase vertexBuffer)
                    {
                        device.VertexDeclaration = effect.VertexDeclaration;
                        device.SpriteBatch.Draw(context.Texture, m, context.Alpha, context.Overlay, primitiveType,
                            vertexBuffer.Vertices, context.Vertex.Offset + startIndex, vertexCount);
                    }
#if BENCHMARK
                }
#endif
            }

            private void DrawNotNormalBlend(PPDDevice device, AlphaBlendContext context, Matrix m, WorkspaceTexture workspaceTexture,
                PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexCount)
            {
#if BENCHMARK
                using (var handler = Benchmark.Instance.Start("Effect Prepare"))
                {
#endif
                    effect.Technique = techniqueNames[new Tuple<BlendMode, bool, bool, ColorFilterType>(context.BlendMode, context.Texture != null && context.Texture.PA,
                        context.MaskTexture != null, context.FilterCount == 0 ? ColorFilterType.None : context.Filters[context.FilterCount - 1].FilterType)];
                    effect.SetTexture(textureHandle, context.Texture);
                    effect.SetTexture(lastRenderTargetTextureHandle, workspaceTexture.Texture);
                    effect.SetTexture(maskTextureHandle, context.MaskTexture);
                    if (context.FilterCount > 0)
                    {
                        effect.SetValue(filterInfoHandle, context.Filters[context.FilterCount - 1].ToFilterInfo());
                    }
                    effect.SetValue(drawInfoHandle, new DrawInfo
                    {
                        Matrix = m,
                        Alpha = context.Alpha,
                        OverlayColor = context.Overlay
                    });
#if BENCHMARK
                }
#endif

#if BENCHMARK
                using (var handler = Benchmark.Instance.Start("Draw"))
                {
#endif
                    var passCount = effect.Begin();
                    effect.BeginPass(0);
                    effect.CommitChanges();
                    device.VertexDeclaration = effect.VertexDeclaration;
                    device.SetStreamSource(context.Vertex.VertexBucket.VertexBuffer);
                    device.DrawPrimitives(primitiveType, context.Vertex.Offset + startIndex, primitiveCount);
                    effect.EndPass();
                    effect.End();
                    effect.SetTexture(textureHandle, null);
#if BENCHMARK
                }
#endif
            }

            protected override void DisposeResource()
            {
                effect?.Dispose();
                effect = null;
                lastRenderTargetTextureHandle?.Dispose();
                lastRenderTargetTextureHandle = null;
                textureHandle?.Dispose();
                textureHandle = null;
                maskTextureHandle?.Dispose();
                maskTextureHandle = null;
                drawInfoHandle?.Dispose();
                drawInfoHandle = null;
                widthHeightHandle?.Dispose();
                widthHeightHandle = null;
                projectionHandle?.Dispose();
                projectionHandle = null;
                filterInfoHandle?.Dispose();
                filterInfoHandle = null;
            }
        }
    }
}
