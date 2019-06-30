namespace PPDFramework.Vertex
{
    /// <summary>
    /// 頂点情報のファクトリーインターフェースです。
    /// </summary>
    public interface IVertexBufferFactory
    {
        /// <summary>
        /// 作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        VertexBufferBase Create(PPDDevice device, int sizeInBytes);
    }
}
