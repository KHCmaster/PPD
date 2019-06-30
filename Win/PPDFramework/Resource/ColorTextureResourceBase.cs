using PPDFramework.Texture;
using SharpDX;

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
