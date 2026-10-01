using System;

namespace PPDFramework.Vertex
{
    /// <summary>
    /// マネージドな頂点バッファークラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    /// <param name="sizeInBytes"></param>
    public class ManagedVertexBuffer(PPDDevice device, int sizeInBytes) : DisposableComponent
    {
        /// <summary>
        /// デバイスです。
        /// </summary>
        protected PPDDevice device = device;

        /// <summary>
        /// 頂点情報です。
        /// </summary>
        public ColoredTexturedVertex[] Vertices
        {
            get;
            private set;
        } = new ColoredTexturedVertex[sizeInBytes / ColoredTexturedVertex.Size];

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

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="offset"></param>
        public virtual void Write(ReadOnlySpan<ColoredTexturedVertex> vertex, int offset)
        {
            vertex.CopyTo(Vertices.AsSpan(offset));
        }
    }
}
