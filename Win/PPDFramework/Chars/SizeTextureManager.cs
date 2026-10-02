using PPDFramework.Texture;
using System.Numerics;
using PPDMath;
using System.Collections.Generic;

namespace PPDFramework.Chars
{
    class SizeTextureManager(PPDDevice device) : DisposableComponent
    {
        readonly PPDDevice device = device;
        readonly List<SizeTexture> textures = [];

        public SizeTexture Write(TextureBase texture, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
        {
            lock (textures)
            {
                foreach (var sizeTexture in textures)
                {
                    if (sizeTexture.Write(texture, width, height, out uv, out uvSize, out usingAvailableSpace))
                    {
                        return sizeTexture;
                    }
                }
                var newSizeTexture = new SizeTexture(device);
                textures.Add(newSizeTexture);
                newSizeTexture.Write(texture, width, height, out uv, out uvSize, out usingAvailableSpace);
                return newSizeTexture;
            }
        }

        /// <summary>
        /// 画素を書き込みます（一時的なテクスチャを作らずに、文字のテクスチャへ直接書き込む）。
        /// </summary>
        public SizeTexture WritePixels(byte[] pixels, int srcWidth, int srcHeight, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
        {
            lock (textures)
            {
                foreach (var sizeTexture in textures)
                {
                    if (sizeTexture.WritePixels(pixels, srcWidth, srcHeight, width, height, out uv, out uvSize, out usingAvailableSpace))
                    {
                        return sizeTexture;
                    }
                }
                var newSizeTexture = new SizeTexture(device);
                textures.Add(newSizeTexture);
                newSizeTexture.WritePixels(pixels, srcWidth, srcHeight, width, height, out uv, out uvSize, out usingAvailableSpace);
                return newSizeTexture;
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            lock (textures)
            {
                foreach (var texture in textures)
                {
                    texture.Dispose();
                }
                textures.Clear();
            }
        }
    }
}
