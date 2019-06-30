using System;

namespace PPDFramework.Vertex
{
    /// <summary>
    /// マネージドな頂点バッファークラスです。
    /// </summary>
    public class ManagedVertexBuffer : DisposableComponent
    {
        /// <summary>
        /// デバイスです。
        /// </summary>
        protected PPDDevice device;

        /// <summary>
        /// 頂点情報です。
        /// </summary>
        public ColoredTexturedVertex[] Vertices
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sizeInBytes"></param>
        public ManagedVertexBuffer(PPDDevice device, int sizeInBytes)
        {
            this.device = device;
            Vertices = new ColoredTexturedVertex[sizeInBytes / ColoredTexturedVertex.Size];
        }

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        public virtual void Write(ColoredTexturedVertex[] vertex, int count, int offset)
        {
            Array.Copy(vertex, 0, Vertices, offset, count);
        }
    }
}
