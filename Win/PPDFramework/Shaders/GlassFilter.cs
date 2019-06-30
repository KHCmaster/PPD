using PPDFramework.Effect;
using PPDFramework.Texture;
using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// ガラスフィルタのクラスです。
    /// </summary>
    public class GlassFilter : DisposableComponent
    {
        EffectBase effect;
        EffectHandleBase widthHeightHandle;
        EffectHandleBase filterTextureHandle;
        EffectHandleBase lastRenderTargetFilterTextureHandle;
        EffectHandleBase projectionHandle;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public GlassFilter(PPDDevice device)
        {
            effect = EffectFactoryManager.Factory.FromMemory(device, device.GetShaderBytecode("Glass"));
            widthHeightHandle = effect.GetParameter("WidthHeight");
            filterTextureHandle = effect.GetParameter("FilterTexture");
            lastRenderTargetFilterTextureHandle = effect.GetParameter("LastRenderTargetTexture");
            projectionHandle = effect.GetParameter("Projection");

            effect.Technique = "Glass";
            effect.SetValue(projectionHandle, device.Projection);
            effect.SetValue(widthHeightHandle, new Vector2(device.Width, device.Height));
        }

        /// <summary>
        /// コピーします。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filterTexture"></param>
        /// <param name="lastRenderTargetTexture"></param>
        public void Draw(PPDDevice device, TextureBase filterTexture, TextureBase lastRenderTargetTexture)
        {
            effect.SetTexture(filterTextureHandle, filterTexture);
            effect.SetTexture(lastRenderTargetFilterTextureHandle, lastRenderTargetTexture);
            var passCount = effect.Begin();
            effect.BeginPass(0);
            effect.CommitChanges();
            device.VertexDeclaration = effect.VertexDeclaration;
            var screenVertex = device.GetModule<ShaderCommon>().ScreenVertex;
            device.SetStreamSource(screenVertex.VertexBucket.VertexBuffer);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, screenVertex.Offset, 2);
            effect.EndPass();
            effect.End();
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
            if (lastRenderTargetFilterTextureHandle != null)
            {
                lastRenderTargetFilterTextureHandle.Dispose();
                lastRenderTargetFilterTextureHandle = null;
            }
            if (projectionHandle != null)
            {
                projectionHandle.Dispose();
                projectionHandle = null;
            }
        }
    }
}
