using System.Collections.Generic;
using System.Threading;

namespace PPDFramework.Vertex
{
    class VertexBucket(PPDDevice device) : DisposableComponent
    {
        const int BucketSize = 1024 * 32;
        int restCount = BucketSize;
        readonly Dictionary<int, Stack<int>> deallocatedSpaces = [];
        readonly Lock restCountLock = new();

        internal VertexBufferBase VertexBuffer
        {
            get;
            private set;
        } = VertexBufferFactoryManager.Factory.Create(device, BucketSize * ColoredTexturedVertex.Size);

        public VertexInfo Allocate(int count)
        {
            var offset = -1;
            lock (deallocatedSpaces)
            {
                if (deallocatedSpaces.TryGetValue(count, out Stack<int> offsets))
                {
                    if (offsets.Count > 0)
                    {
                        offset = offsets.Pop();
                    }
                }
            }
            if (offset < 0)
            {
                lock (restCountLock)
                {
                    if (count > restCount)
                    {
                        return null;
                    }
                    offset = BucketSize - restCount;
                    restCount -= count;
                }
            }

            return new VertexInfo(this, count, offset);
        }

        public void Deallocate(VertexInfo vertexInfo)
        {
            lock (deallocatedSpaces)
            {
                if (!deallocatedSpaces.TryGetValue(vertexInfo.Count, out Stack<int> offsets))
                {
                    offsets = new Stack<int>();
                    deallocatedSpaces.Add(vertexInfo.Count, offsets);
                }
                offsets.Push(vertexInfo.Offset);
            }
        }

        protected override void DisposeResource()
        {
            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }
    }
}
