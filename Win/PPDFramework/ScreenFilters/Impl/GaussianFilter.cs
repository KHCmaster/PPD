using PPDFramework.Effect;
using SharpDX;

namespace PPDFramework.ScreenFilters.Impl
{
    /// <summary>
    /// ガウスフィルタを行います。
    /// </summary>
    public class GaussianFilter : DisposableComponent
    {
        EffectBase effect;
        EffectHandleBase widthHeightHandle;
        EffectHandleBase projectionHandle;
        EffectHandleBase weightsHandle;
        EffectHandleBase textureHandle;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="device"></param>
        public GaussianFilter(PPDDevice device)
        {
            effect = EffectFactoryManager.Factory.FromMemory(device, device.GetShaderBytecode("GaussianFilter"));
            widthHeightHandle = effect.GetParameter("WidthHeight");
            projectionHandle = effect.GetParameter("Projection");
            weightsHandle = effect.GetParameter("Weights");
            textureHandle = effect.GetParameter("Texture");

            effect.Technique = "Gaussian";
            effect.SetValue(projectionHandle, device.Projection);
            effect.SetValue(widthHeightHandle, new Vector2(device.Width, device.Height));
        }

        /// <summary>
        /// 描画します
        /// </summary>
        /// <param name="device"></param>
        /// <param name="weights"></param>
        public void Draw(PPDDevice device, float[] weights)
        {
            if (PPDSetting.Setting.ShaderDisabled)
            {
                return;
            }
            using (var tempWorkSpace = device.Workspace.Get())
            {
                effect.SetValue(weightsHandle, weights);
                effect.SetTexture(textureHandle, device.GetRenderTarget().Texture);
                var renderTarget = device.GetRenderTarget();
                device.SetRenderTarget(tempWorkSpace);
                device.Clear();
                var passCount = effect.Begin();
                effect.BeginPass(0);
                DrawQuad(device);
                effect.EndPass();
                device.SetRenderTarget(renderTarget);
                device.Clear();
                effect.SetTexture(textureHandle, tempWorkSpace.Texture);
                effect.BeginPass(1);
                DrawQuad(device);
                effect.EndPass();
                effect.End();
            }
        }

        private void DrawQuad(PPDDevice device)
        {
            effect.CommitChanges();
            device.VertexDeclaration = effect.VertexDeclaration;
            var screenVertex = device.GetModule<ShaderCommon>().ScreenVertex;
            device.SetStreamSource(screenVertex.VertexBucket.VertexBuffer);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, screenVertex.Offset, 2);
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
            if (widthHeightHandle != null)
            {
                widthHeightHandle.Dispose();
                widthHeightHandle = null;
            }
            if (projectionHandle != null)
            {
                projectionHandle.Dispose();
                projectionHandle = null;
            }
            if (weightsHandle != null)
            {
                weightsHandle.Dispose();
                weightsHandle = null;
            }
            if (textureHandle != null)
            {
                textureHandle.Dispose();
                textureHandle = null;
            }
        }
    }
}
