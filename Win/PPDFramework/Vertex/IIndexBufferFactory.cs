namespace PPDFramework.Vertex
{
    /// <summary>
    /// 頂点情報のファクトリーインターフェースです。
    /// </summary>
    public interface IIndexBufferFactory
    {
        /// <summary>
        /// 作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        IndexBufferBase Create(PPDDevice device, int sizeInBytes);
    }
}
