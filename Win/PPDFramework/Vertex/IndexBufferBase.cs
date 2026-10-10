namespace PPDFramework.Vertex
{
    /// <summary>
    /// インデックスバッファーの基底クラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    public abstract class IndexBufferBase(PPDDevice device) : DisposableComponent
    {
        /// <summary>
        /// デバイスです。
        /// </summary>
        protected PPDDevice device = device;

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        public abstract void Write(int[] indices, int count, int offset);
    }
}
