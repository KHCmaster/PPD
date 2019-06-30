using SharpDX;

namespace PPDFramework.Texture.DX11
{
    class TextureData : TextureDataBase
    {
        SharpDX.Direct3D11.Texture2D texture;
        DataStream dataStream;

        public override DataStream DataStream
        {
            get { return dataStream; }
        }

        public TextureData(SharpDX.Direct3D11.Texture2D texture)
        {
            this.texture = texture;
            // TODO lock
            dataStream = new DataStream(null);
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            // TODO unlock
        }
    }
}
