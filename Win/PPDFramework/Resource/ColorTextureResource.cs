using PPDFramework.Texture;
using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;
using Color = Vortice.Mathematics.Color;

namespace PPDFramework.Resource
{
    /// <summary>
    /// 色のテクスチャリソースクラスです
    /// </summary>
    /// <remarks>
    /// コンストラクタです
    /// </remarks>
    /// <param name="device"></param>
    /// <param name="color"></param>
    public class ColorTextureResource(PPDDevice device, Color4 color) : ColorTextureResourceBase
    {
        readonly TextureBase texture = device.GetModule<ColorTextureAllcator>().CreateTexture(color);
        readonly Color4 color = color;

        /// <summary>
        /// テクスチャを取得します
        /// </summary>
        public override TextureBase Texture
        {
            get
            {
                return texture;
            }
        }

        /// <summary>
        /// 色を取得します
        /// </summary>
        public override Color4 Color
        {
            get
            {
                return color;
            }
        }

        /// <summary>
        /// 破棄します
        /// </summary>
        protected override void DisposeResource()
        {
            // ColorTextureAllcatorでキャッシュ済み
        }
    }
}
