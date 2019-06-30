namespace PPDFramework.Vertex.DX9
{
    class VertexBufferFactory : IVertexBufferFactory
    {
        public VertexBufferBase Create(PPDDevice device, int sizeInBytes)
        {
            return new VertexBuffer(device, new SharpDX.Direct3D9.VertexBuffer((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, sizeInBytes, SharpDX.Direct3D9.Usage.WriteOnly, SharpDX.Direct3D9.VertexFormat.None, SharpDX.Direct3D9.Pool.Managed), sizeInBytes);
        }
    }
}
