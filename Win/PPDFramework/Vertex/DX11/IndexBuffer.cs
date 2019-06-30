using System;

namespace PPDFramework.Vertex.DX11
{
    class IndexBuffer : IndexBufferBase
    {
        public SharpDX.Direct3D11.Buffer _Buffer
        {
            get;
            private set;
        }

        public IndexBuffer(PPDDevice device, SharpDX.Direct3D11.Buffer buffer) : base(device)
        {
            _Buffer = buffer;
        }

        public override void Write(int[] indices, int count, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
