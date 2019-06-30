using PPDFramework.Texture;
using SharpDX;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// 1x1の色付きテクスチャを作成するクラスです
    /// </summary>
    public class ColorTextureAllcator : DisposableComponent
    {
        PPDDevice device;
        Dictionary<Color4, TextureBase> cache = new Dictionary<Color4, TextureBase>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public ColorTextureAllcator(PPDDevice device)
        {
            this.device = device;
        }

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
            texture.Write(new byte[] { (byte)(255 * color.Blue), (byte)(255 * color.Green), (byte)(255 * color.Red), (byte)(255 * color.Alpha) });
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
