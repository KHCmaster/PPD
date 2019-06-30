namespace PPDFramework.Vertex
{
    /// <summary>
    /// インデックスバッファーの基底クラスです。
    /// </summary>
    public abstract class IndexBufferBase : DisposableComponent
    {
        /// <summary>
        /// デバイスです。
        /// </summary>
        protected PPDDevice device;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        protected IndexBufferBase(PPDDevice device)
        {
            this.device = device;
        }

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        public abstract void Write(int[] indices, int count, int offset);
    }
}
