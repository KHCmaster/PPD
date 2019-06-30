namespace PPDFramework.Vertex
{
    /// <summary>
    /// 頂点情報の基底クラスです。
    /// </summary>
    public abstract class VertexBufferBase : ManagedVertexBuffer
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sizeInBytes"></param>
        protected VertexBufferBase(PPDDevice device, int sizeInBytes) : base(device, sizeInBytes)
        {
        }
    }
}
