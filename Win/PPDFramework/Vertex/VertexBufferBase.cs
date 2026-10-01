namespace PPDFramework.Vertex
{
    /// <summary>
    /// 頂点情報の基底クラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    /// <param name="sizeInBytes"></param>
    public abstract class VertexBufferBase(PPDDevice device, int sizeInBytes) : ManagedVertexBuffer(device, sizeInBytes)
    {
    }
}
