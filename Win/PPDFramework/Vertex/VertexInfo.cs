namespace PPDFramework.Vertex
{
    /// <summary>
    /// 頂点情報です。
    /// </summary>
    public class VertexInfo : DisposableComponent
    {
        internal VertexBucket VertexBucket
        {
            get;
            private set;
        }

        /// <summary>
        /// 頂点数を取得します。
        /// </summary>
        public int Count
        {
            get;
            private set;
        }

        internal int Offset
        {
            get;
            private set;
        }

        internal VertexInfo(VertexBucket bucket, int count, int offset)
        {
            VertexBucket = bucket;
            Count = count;
            Offset = offset;
        }

        /// <summary>
        /// 頂点情報を書き込みます。
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="offset"></param>
        public void Write(ColoredTexturedVertex[] vertex, int offset = 0)
        {
            Write(vertex, vertex.Length, offset);
        }

        /// <summary>
        /// 頂点情報を書き込みます。
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        public void Write(ColoredTexturedVertex[] vertex, int count, int offset = 0)
        {
            VertexBucket.VertexBuffer.Write(vertex, count, Offset + offset);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (VertexBucket != null)
            {
                VertexBucket.Deallocate(this);
                VertexBucket = null;
            }
            base.DisposeResource();
        }
    }
}
