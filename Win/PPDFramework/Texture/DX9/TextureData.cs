using SharpDX;

namespace PPDFramework.Texture.DX9
{
    class TextureData : TextureDataBase
    {
        SharpDX.Direct3D9.Texture texture;
        DataStream dataStream;

        public override DataStream DataStream
        {
            get { return dataStream; }
        }

        public TextureData(SharpDX.Direct3D9.Texture texture)
        {
            this.texture = texture;
            texture.LockRectangle(0, SharpDX.Direct3D9.LockFlags.Discard, out dataStream);
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            texture.UnlockRectangle(0);
        }
    }
}
