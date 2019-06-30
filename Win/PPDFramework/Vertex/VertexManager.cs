using System.Collections.Generic;

namespace PPDFramework.Vertex
{
    class VertexManager : DisposableComponent
    {
        const int InitialBucketCount = 8;

        PPDDevice device;
        List<VertexBucket> buckets;

        public VertexManager(PPDDevice device)
        {
            this.device = device;
            buckets = new List<VertexBucket>(InitialBucketCount);
            for (var i = 0; i < InitialBucketCount; i++)
            {
                buckets.Add(new VertexBucket(device));
            }
        }

        public VertexInfo Allocate(int count)
        {
            var ret = AllocateImpl(count);
            if (ret == null)
            {
                Resize();
                ret = AllocateImpl(count);
            }
            return ret;
        }

        private VertexInfo AllocateImpl(int count)
        {
            foreach (var bucket in buckets)
            {
                var ret = bucket.Allocate(count);
                if (ret != null)
                {
                    return ret;
                }
            }
            return null;
        }

        private void Resize()
        {
            var addCount = buckets.Count;
            for (var i = 0; i < addCount; i++)
            {
                buckets.Add(new VertexBucket(device));
            }
        }

        protected override void DisposeResource()
        {
            if (buckets != null)
            {
                foreach (var bucket in buckets)
                {
                    bucket.Dispose();
                }
                buckets = null;
            }
        }
    }
}
