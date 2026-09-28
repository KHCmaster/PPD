using SharpDX;
using System.Runtime.InteropServices;

namespace PPDFramework.Texture.DX11
{
    class TextureData : TextureDataBase
    {
        Texture texture;
        GCHandle handle;
        DataStream dataStream;

        public override DataStream DataStream
        {
            get { return dataStream; }
        }

        public TextureData(Texture texture)
        {
            this.texture = texture;
            var data = texture.BeginCpuAccess();
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            dataStream = new DataStream(handle.AddrOfPinnedObject(), data.Length, true, true);
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            dataStream.Dispose();
            handle.Free();
            texture.EndCpuAccess();
        }
    }
}
