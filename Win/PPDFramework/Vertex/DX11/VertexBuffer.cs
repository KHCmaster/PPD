using SharpDX;
using SharpDX.Direct3D11;

namespace PPDFramework.Vertex.DX11
{
    class VertexBuffer : VertexBufferBase
    {
        public SharpDX.Direct3D11.Buffer _Buffer
        {
            get;
            private set;
        }

        public VertexBuffer(PPDDevice device, SharpDX.Direct3D11.Buffer buffer, int sizeInBytes) : base(device, sizeInBytes)
        {
            _Buffer = buffer;
        }

        public override void Write(ColoredTexturedVertex[] vertex, int count, int offset)
        {
            base.Write(vertex, count, offset);
            var databox = ((PPDFramework.DX11.PPDDevice)device).Context.MapSubresource(_Buffer, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
            dataStream.Position = ColoredTexturedVertex.Size * offset;
            dataStream.WriteRange(vertex, 0, count);
            dataStream.Close();
            dataStream.Dispose();
            ((PPDFramework.DX11.PPDDevice)device).Context.UnmapSubresource(_Buffer, 0);
        }

        protected override void DisposeResource()
        {
            if (_Buffer != null)
            {
                _Buffer.Dispose();
                _Buffer = null;
            }
        }
    }
}
