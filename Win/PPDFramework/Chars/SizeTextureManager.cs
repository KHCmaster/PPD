using PPDFramework.Texture;
using SharpDX;
using System.Collections.Generic;

namespace PPDFramework.Chars
{
    class SizeTextureManager : DisposableComponent
    {
        PPDDevice device;
        List<SizeTexture> textures;

        public SizeTextureManager(PPDDevice device)
        {
            this.device = device;
            textures = new List<SizeTexture>();
        }

        public SizeTexture Write(TextureBase texture, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
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

        protected override void DisposeResource()
        {
            base.DisposeResource();
            foreach (var texture in textures)
            {
                texture.Dispose();
            }
            textures.Clear();
        }
    }
}
