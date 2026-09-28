using SharpDX.Direct3D11;

namespace PPDFramework.Vertex.DX11
{
    class VertexBufferFactory : IVertexBufferFactory
    {
        public VertexBufferBase Create(PPDDevice device, int sizeInBytes)
        {
            return new VertexBuffer(device, new Buffer((SharpDX.Direct3D11.Device)device.Device, new BufferDescription
            {
                SizeInBytes = sizeInBytes,
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.VertexBuffer,
                Usage = ResourceUsage.Default
            }), sizeInBytes);
        }
    }
}
