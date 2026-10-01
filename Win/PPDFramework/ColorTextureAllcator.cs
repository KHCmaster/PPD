using PPDFramework.Texture;
using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// 1x1の色付きテクスチャを作成するクラスです
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    public class ColorTextureAllcator(PPDDevice device) : DisposableComponent
    {
        readonly PPDDevice device = device;
        readonly Dictionary<Color4, TextureBase> cache = [];

        /// <summary>
        /// テクスチャを作成します
        /// </summary>
        /// <param name="color">色</param>
        /// <returns>テクスチャ</returns>
        public TextureBase CreateTexture(Color4 color)
        {
            return CreateTexture(color, true);
        }

        /// <summary>
        /// テクスチャを作成します
        /// </summary>
        /// <param name="color">色</param>
        /// <param name="withCache">キャッシュするかどうか</param>
        /// <returns>テクスチャ</returns>
        public TextureBase CreateTexture(Color4 color, bool withCache)
        {
            if (withCache)
            {
                lock (cache)
                {
                    if (cache.ContainsKey(color))
                    {
                        return cache[color];
                    }
                }
            }
            var texture = TextureFactoryManager.Factory.Create(device, 1, 1, 1, true);
            texture.Write([(byte)(255 * color.Blue), (byte)(255 * color.Green), (byte)(255 * color.Red), (byte)(255 * color.Alpha)]);
            if (withCache)
            {
                lock (cache)
                {
                    cache[color] = texture;
                }
            }
            return texture;
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            foreach (var c in cache)
            {
                c.Value.Dispose();
            }
            cache.Clear();

        }
    }
}
