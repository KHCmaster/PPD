using PPDFramework.Effect;
using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// モザイクフィルターのクラスです。
    /// </summary>
    public class MosaicFilter : DisposableComponent
    {
        EffectBase effect;
        EffectHandleBase widthHeightHandle;
        EffectHandleBase filterTextureHandle;
        EffectHandleBase lastRenderTargetTextureHandle;
        EffectHandleBase xTextureHandle;
        EffectHandleBase projectionHandle;
        EffectHandleBase sizeHandle;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public MosaicFilter(PPDDevice device)
        {
            effect = EffectFactoryManager.Factory.FromMemory(device, device.GetShaderBytecode("Mosaic"));
            widthHeightHandle = effect.GetParameter("WidthHeight");
            filterTextureHandle = effect.GetParameter("FilterTexture");
            lastRenderTargetTextureHandle = effect.GetParameter("LastRenderTargetTexture");
            xTextureHandle = effect.GetParameter("XTexture");
            projectionHandle = effect.GetParameter("Projection");
            sizeHandle = effect.GetParameter("Size");

            effect.SetValue(projectionHandle, device.Projection);
            effect.SetValue(widthHeightHandle, new Vector2(device.Width, device.Height));
        }

        /// <summary>
        /// コピーします。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filterTexture"></param>
        /// <param name="size"></param>
        public void Draw(PPDDevice device, WorkspaceTexture filterTexture, int size)
        {
            using (var workspaceTexture = device.Workspace.Get())
            using (var xTexture = device.Workspace.Get())
            {
#if BENCHMARK
                using (var handler = Benchmark.Instance.Start("Copy"))
                {
#endif
                    device.StretchRectangle(device.GetRenderTarget(), workspaceTexture);
#if BENCHMARK
                }
#endif
                var renderTarget = device.GetRenderTarget();
                device.SetRenderTarget(xTexture);
                device.Clear();
                effect.Technique = "Mosaic";
                effect.SetTexture(filterTextureHandle, filterTexture.Texture);
                effect.SetTexture(lastRenderTargetTextureHandle, workspaceTexture.Texture);
                effect.SetValue(sizeHandle, size);
                var passCount = effect.Begin();
                effect.BeginPass(0);
                effect.CommitChanges();
                device.VertexDeclaration = effect.VertexDeclaration;
                var screenVertex = device.GetModule<ShaderCommon>().ScreenVertex;
                device.SetStreamSource(screenVertex.VertexBucket.VertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, screenVertex.Offset, 2);
                effect.EndPass();
                device.SetRenderTarget(renderTarget);
                effect.SetTexture(xTextureHandle, xTexture.Texture);
                effect.BeginPass(1);
                effect.CommitChanges();
                device.DrawPrimitives(PrimitiveType.TriangleStrip, screenVertex.Offset, 2);
                effect.EndPass();
                effect.End();
            }
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
            if (filterTextureHandle != null)
            {
                filterTextureHandle.Dispose();
                filterTextureHandle = null;
            }
            if (lastRenderTargetTextureHandle != null)
            {
                lastRenderTargetTextureHandle.Dispose();
                lastRenderTargetTextureHandle = null;
            }
            if (xTextureHandle != null)
            {
                xTextureHandle.Dispose();
                xTextureHandle = null;
            }
            if (projectionHandle != null)
            {
                projectionHandle.Dispose();
                projectionHandle = null;
            }
            if (sizeHandle != null)
            {
                sizeHandle.Dispose();
                sizeHandle = null;
            }
        }
    }
}