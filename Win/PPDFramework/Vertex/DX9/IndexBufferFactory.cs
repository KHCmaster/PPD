namespace PPDFramework.Vertex.DX9
{
    class IndexBufferFactory : IIndexBufferFactory
    {
        public IndexBufferBase Create(PPDDevice device, int sizeInBytes)
        {
            return new IndexBuffer(device, new SharpDX.Direct3D9.IndexBuffer((SharpDX.Direct3D9.Device)device.Device, sizeInBytes, SharpDX.Direct3D9.Usage.WriteOnly, SharpDX.Direct3D9.Pool.Managed, false));
        }
    }
}
