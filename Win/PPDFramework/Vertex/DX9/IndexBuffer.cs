namespace PPDFramework.Vertex.DX9
{
    class IndexBuffer : IndexBufferBase
    {
        public SharpDX.Direct3D9.IndexBuffer _IndexBuffer
        {
            get;
            private set;
        }

        public IndexBuffer(PPDDevice device, SharpDX.Direct3D9.IndexBuffer indexBuffer) : base(device)
        {
            _IndexBuffer = indexBuffer;
        }

        public override void Write(int[] indices, int count, int offset)
        {
            var dataStream = _IndexBuffer.Lock(offset * sizeof(int), count * sizeof(int), SharpDX.Direct3D9.LockFlags.None);
            dataStream.WriteRange(indices, 0, count);
            _IndexBuffer.Unlock();
        }
    }
}
