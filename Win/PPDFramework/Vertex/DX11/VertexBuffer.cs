using Vortice.Direct3D11;
using Vortice.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace PPDFramework.Vertex.DX11
{
    /// <summary>
    /// DirectX11の頂点バッファーです。
    /// 書き込まれた頂点はCPU側に保持し、描画スレッドで使われるときにGPUへ転送します。
    /// </summary>
    class VertexBuffer : VertexBufferBase
    {
        readonly object writeLock = new object();
        int dirtyStart;
        int dirtyEnd;

        public ID3D11Buffer _Buffer
        {
            get;
            private set;
        }

        public VertexBuffer(PPDDevice device, ID3D11Buffer buffer, int sizeInBytes) : base(device, sizeInBytes)
        {
            _Buffer = buffer;
        }

        public override void Write(ColoredTexturedVertex[] vertex, int count, int offset)
        {
            lock (writeLock)
            {
                base.Write(vertex, count, offset);
                if (dirtyEnd <= dirtyStart)
                {
                    dirtyStart = offset;
                    dirtyEnd = offset + count;
                }
                else
                {
                    dirtyStart = Math.Min(dirtyStart, offset);
                    dirtyEnd = Math.Max(dirtyEnd, offset + count);
                }
            }
        }

        /// <summary>
        /// CPU側で書き込まれた頂点をGPUへ転送します。描画スレッドから呼ぶ必要があります。
        /// </summary>
        /// <param name="context"></param>
        internal void Flush(ID3D11DeviceContext context)
        {
            if (dirtyEnd <= dirtyStart)
            {
                return;
            }
            lock (writeLock)
            {
                if (_Buffer == null || dirtyEnd <= dirtyStart)
                {
                    return;
                }
                var handle = GCHandle.Alloc(Vertices, GCHandleType.Pinned);
                try
                {
                    var ptr = handle.AddrOfPinnedObject() + dirtyStart * ColoredTexturedVertex.Size;
                    context.UpdateSubresource(_Buffer, 0,
                        new Box(dirtyStart * ColoredTexturedVertex.Size, 0, 0, dirtyEnd * ColoredTexturedVertex.Size, 1, 1),
                        ptr, 0, 0);
                }
                finally
                {
                    handle.Free();
                }
                dirtyStart = dirtyEnd = 0;
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
