using Vortice.Direct3D11;
using Vortice.Mathematics;
using System.Runtime.InteropServices;

namespace PPDFramework.Vertex.DX11
{
    /// <summary>
    /// DirectX11のインデックスバッファーです。描画スレッドから書き込む必要があります。
    /// </summary>
    class IndexBuffer(PPDDevice device, ID3D11Buffer buffer) : IndexBufferBase(device)
    {
        public ID3D11Buffer Buffer
        {
            get;
            private set;
        } = buffer;

        public override void Write(int[] indices, int count, int offset)
        {
            if (count <= 0)
            {
                return;
            }
            var handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            try
            {
                ((PPDFramework.DX11.PPDDevice)device).Context.UpdateSubresource(Buffer, 0,
                    new Box(offset * sizeof(int), 0, 0, (offset + count) * sizeof(int), 1, 1),
                    handle.AddrOfPinnedObject(), 0, 0);
            }
            finally
            {
                handle.Free();
            }
        }

        protected override void DisposeResource()
        {
            Buffer?.Dispose();
            Buffer = null;
        }
    }
}
