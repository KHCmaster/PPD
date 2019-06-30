using System.IO;

namespace PPDFramework.Texture
{
    /// <summary>
    /// テクスチャのファクトリーインターフェースです。
    /// </summary>
    public interface ITextureFactory
    {
        /// <summary>
        /// 作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="level"></param>
        /// <param name="pa"></param>
        /// <returns></returns>
        TextureBase Create(PPDDevice device, int width, int height, int level, bool pa);

        /// <summary>
        /// レンダーターゲットを作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="level"></param>
        /// <param name="pa"></param>
        /// <returns></returns>
        TextureBase CreateRenderTarget(PPDDevice device, int width, int height, int level, bool pa);

        /// <summary>
        /// ストリームから作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="stream"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pa"></param>
        /// <returns></returns>
        TextureBase FromStream(PPDDevice device, Stream stream, int width, int height, bool pa);

        /// <summary>
        /// ファイルから作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pa"></param>
        /// <returns></returns>
        TextureBase FromFile(PPDDevice device, string filename, int width, int height, bool pa);
    }
}
