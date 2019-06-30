using SharpDX.Direct3D9;

namespace PPDFramework.Vertex.DX9
{
    class VertexBuffer : VertexBufferBase
    {
        public SharpDX.Direct3D9.VertexBuffer _VertexBuffer
        {
            get;
            private set;
        }

        public VertexBuffer(PPDDevice device, SharpDX.Direct3D9.VertexBuffer vertexBuffer, int sizeInBytes) : base(device, sizeInBytes)
        {
            _VertexBuffer = vertexBuffer;
        }

        public override void Write(ColoredTexturedVertex[] vertex, int count, int offset)
        {
            base.Write(vertex, count, offset);
            var dataStream = _VertexBuffer.Lock(offset * ColoredTexturedVertex.Size, count * ColoredTexturedVertex.Size, LockFlags.None);
            dataStream.WriteRange(vertex, 0, count);
            _VertexBuffer.Unlock();
        }

        protected override void DisposeResource()
        {
            if (_VertexBuffer != null)
            {
                _VertexBuffer.Dispose();
                _VertexBuffer = null;
            }
        }
    }
}
