using PPDFramework.Texture;
using SharpDX;
using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;
using Color = Vortice.Mathematics.Color;

namespace PPDFramework.Resource
{
    /// <summary>
    /// 色のテクスチャリソースクラスです
    /// </summary>
    public class ColorTextureResource : ColorTextureResourceBase
    {
        TextureBase texture;
        Color4 color;

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
        /// コンストラクタです
        /// </summary>
        /// <param name="device"></param>
        /// <param name="color"></param>
        public ColorTextureResource(PPDDevice device, Color4 color)
        {
            this.color = color;
            texture = device.GetModule<ColorTextureAllcator>().CreateTexture(color);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~ColorTextureResource()
        {
            Dispose();
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
