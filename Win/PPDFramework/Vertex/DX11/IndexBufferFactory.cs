using Vortice.Direct3D11;

namespace PPDFramework.Vertex.DX11
{
    class IndexBufferFactory : IIndexBufferFactory
    {
        public IndexBufferBase Create(PPDDevice device, int sizeInBytes)
        {
            return new IndexBuffer(device, ((ID3D11Device)device.Device).CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)sizeInBytes,
                CPUAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.IndexBuffer,
                Usage = ResourceUsage.Default
            }, (SubresourceData?)null));
        }
    }
}
