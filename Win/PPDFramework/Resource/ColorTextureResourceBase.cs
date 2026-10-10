using PPDFramework.Texture;
using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;
using Color = Vortice.Mathematics.Color;

namespace PPDFramework.Resource
{
    /// <summary>
    /// 色のテクスチャリソースのクラスです。
    /// </summary>
    public abstract class ColorTextureResourceBase : ResourceBase
    {
        /// <summary>
        /// テクスチャを取得します。
        /// </summary>
        public abstract TextureBase Texture { get; }

        /// <summary>
        /// カラーを取得します。
        /// </summary>
        public abstract Color4 Color { get; }
    }
}
