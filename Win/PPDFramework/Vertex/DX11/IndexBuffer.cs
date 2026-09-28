using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace PPDFramework.Vertex.DX11
{
    /// <summary>
    /// DirectX11のインデックスバッファーです。描画スレッドから書き込む必要があります。
    /// </summary>
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
            if (count <= 0)
            {
                return;
            }
            var handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            try
            {
                ((PPDFramework.DX11.PPDDevice)device).Context.UpdateSubresource(_Buffer, 0,
                    new ResourceRegion(offset * sizeof(int), 0, 0, (offset + count) * sizeof(int), 1, 1),
                    handle.AddrOfPinnedObject(), 0, 0);
            }
            finally
            {
                handle.Free();
            }
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
