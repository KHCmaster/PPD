using SharpDX.Direct3D11;

namespace PPDFramework.Vertex.DX11
{
    class IndexBufferFactory : IIndexBufferFactory
    {
        public IndexBufferBase Create(PPDDevice device, int sizeInBytes)
        {
            return new IndexBuffer(device, new Buffer((SharpDX.Direct3D11.Device)((PPDFramework.DX11.PPDDevice)device).Device, new BufferDescription
            {
                SizeInBytes = sizeInBytes,
                CpuAccessFlags = CpuAccessFlags.Write,
                BindFlags = BindFlags.IndexBuffer,
                Usage = ResourceUsage.Dynamic
            }));
        }
    }
}
