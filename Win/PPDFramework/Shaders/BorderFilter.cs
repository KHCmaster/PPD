using PPDFramework.Effect;
using SharpDX;
using System;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// ボーダーフィルタのクラスです。
    /// </summary>
    public class BorderFilter : DisposableComponent
    {
        EffectBase effect;
        EffectHandleBase widthHeightHandle;
        EffectHandleBase filterTextureHandle;
        EffectHandleBase lastRenderTargetTextureHandle;
        EffectHandleBase projectionHandle;
        EffectHandleBase colorHandle;
        EffectHandleBase thicknessHandle;
        EffectHandleBase thickness2Handle;
        EffectHandleBase actualThicknessHandle;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public BorderFilter(PPDDevice device)
        {
            effect = EffectFactoryManager.Factory.FromMemory(device, device.GetShaderBytecode("Border"));
            widthHeightHandle = effect.GetParameter("WidthHeight");
            filterTextureHandle = effect.GetParameter("FilterTexture");
            lastRenderTargetTextureHandle = effect.GetParameter("LastRenderTargetTexture");
            projectionHandle = effect.GetParameter("Projection");
            colorHandle = effect.GetParameter("Color");
            thicknessHandle = effect.GetParameter("Thickness");
            thickness2Handle = effect.GetParameter("Thickness2");
            actualThicknessHandle = effect.GetParameter("ActualThickness");

            effect.SetValue(projectionHandle, device.Projection);
            effect.SetValue(widthHeightHandle, new Vector2(device.Width, device.Height));
        }

        /// <summary>
        /// コピーします。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filterTexture"></param>
        /// <param name="border"></param>
        public void Draw(PPDDevice device, WorkspaceTexture filterTexture, Border border)
        {
            if (border.Type == BorderType.Center)
            {
                Draw(device, filterTexture, border, BorderType.Outside, border.Thickness / 2);
                Draw(device, filterTexture, border, BorderType.Inside, border.Thickness / 2);
            }
            else
            {
                Draw(device, filterTexture, border, border.Type, border.Thickness);
            }
        }

        private void Draw(PPDDevice device, WorkspaceTexture filterTexture, Border border, BorderType type, float thickness)
        {
            using (var workspaceTexture = device.Workspace.Get())
            {
#if BENCHMARK
                using (var handler = Benchmark.Instance.Start("Copy"))
                {
#endif
                    device.StretchRectangle(device.GetRenderTarget(), workspaceTexture);
#if BENCHMARK
                }
#endif
                effect.Technique = String.Format("{0}{1}Border", type, border.Blend);
                effect.SetTexture(filterTextureHandle, filterTexture.Texture);
                effect.SetTexture(lastRenderTargetTextureHandle, workspaceTexture.Texture);
                effect.SetValue(colorHandle, border.Color);
                var intThickness = (int)Math.Ceiling(thickness * 2 + 1);
                effect.SetValue(thicknessHandle, (float)intThickness);
                effect.SetValue(thickness2Handle, (int)Math.Pow(intThickness, 2));
                effect.SetValue(actualThicknessHandle, thickness);
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
            if (projectionHandle != null)
            {
                projectionHandle.Dispose();
                projectionHandle = null;
            }
            if (thicknessHandle != null)
            {
                thicknessHandle.Dispose();
                thicknessHandle = null;
            }
            if (thickness2Handle != null)
            {
                thickness2Handle.Dispose();
                thickness2Handle = null;
            }
            if (actualThicknessHandle != null)
            {
                actualThicknessHandle.Dispose();
                actualThicknessHandle = null;
            }
        }
    }
}
