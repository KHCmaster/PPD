using System.IO;

namespace PPDFramework.Resource
{
    /// <summary>
    /// 画像リソースのファクトリインターフェースです。
    /// </summary>
    public interface IImageResourceFactory
    {
        /// <summary>
        /// 作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename"></param>
        /// <param name="pa"></param>
        /// <returns></returns>
        ImageResourceBase Create(PPDDevice device, string filename, bool pa);

        /// <summary>
        /// 作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="stream"></param>
        /// <param name="pa"></param>
        /// <returns></returns>
        ImageResourceBase Create(PPDDevice device, Stream stream, bool pa);
    }
}
