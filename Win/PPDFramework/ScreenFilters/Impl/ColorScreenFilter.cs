using Effect2D;
using PPDFramework.Shaders;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace PPDFramework.ScreenFilters.Impl
{
    /// <summary>
    /// カラースクリーンフィルタのクラスです。
    /// </summary>
    public class ColorScreenFilter : DisposableComponent
    {
        /// <summary>
        /// 描画します
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filters"></param>
        public void Draw(PPDDevice device, IEnumerable<ColorFilterBase> filters)
        {
            if (PPDSetting.Setting.ShaderDisabled)
            {
                return;
            }
            using (var temp = device.Workspace.Get())
            {
                var context = device.GetModule<AlphaBlendContextCache>().Get();
                context.Alpha = 1;
                context.BlendMode = BlendMode.Normal;
                context.Vertex = device.GetModule<ShaderCommon>().ScreenVertex;
                context.Texture = temp.Texture;
                context.SetSRT(Matrix.Identity, 0);
                context.FilterCount = 1;
                context.Transparent = true;
                context.WorldDisabled = true;
                foreach (var filter in filters.Reverse())
                {
                    device.StretchRectangle(device.GetRenderTarget(), temp);
                    context.SetFilter(filter, 0);
                    device.GetModule<AlphaBlend>().Draw(device, context);
                }
            }
        }
    }
}
