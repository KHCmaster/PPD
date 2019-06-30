using System.Collections.Generic;

namespace PPDFramework.Vertex
{
    class VertexBucket : DisposableComponent
    {
        const int BucketSize = 1024 * 32;
        int restCount;
        Dictionary<int, Stack<int>> deallocatedSpaces;
        readonly object restCountLock = new object();

        internal VertexBufferBase VertexBuffer
        {
            get;
            private set;
        }

        public VertexBucket(PPDDevice device)
        {
            VertexBuffer = VertexBufferFactoryManager.Factory.Create(device, BucketSize * ColoredTexturedVertex.Size);
            restCount = BucketSize;
            deallocatedSpaces = new Dictionary<int, Stack<int>>();
        }

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
            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }
        }
    }
}
