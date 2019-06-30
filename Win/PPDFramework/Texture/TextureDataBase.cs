using SharpDX;

namespace PPDFramework.Texture
{
    /// <summary>
    /// テクスチャのデータのインターフェースです。
    /// </summary>
    public abstract class TextureDataBase : DisposableComponent
    {
        /// <summary>
        /// データストリームを取得します。
        /// </summary>
        public abstract DataStream DataStream
        {
            get;
        }
    }
}
