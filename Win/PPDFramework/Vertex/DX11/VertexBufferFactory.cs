using Vortice.Direct3D11;

namespace PPDFramework.Vertex.DX11
{
    class VertexBufferFactory : IVertexBufferFactory
    {
        public VertexBufferBase Create(PPDDevice device, int sizeInBytes)
        {
            return new VertexBuffer(device, ((ID3D11Device)device.Device).CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)sizeInBytes,
                CPUAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.VertexBuffer,
                Usage = ResourceUsage.Default
            }, (SubresourceData?)null), sizeInBytes);
        }
    }
}
