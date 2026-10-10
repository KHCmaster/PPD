using PPDPack;
using System.Collections.Generic;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// PPDPackのスプライトマネージャークラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="packReader"></param>
    /// <param name="imageNames"></param>
    /// <param name="spriteDirName"></param>
    public abstract class PPDPackSpriteManagerBase(PackReader packReader, string[] imageNames, string spriteDirName) : SpriteManagerBase(spriteDirName)
    {
        readonly string[] imageNames = imageNames;
        readonly PackReader packReader = packReader;

        /// <summary>
        /// 画像の情報の一覧を取得します。
        /// </summary>
        /// <returns></returns>
        protected override ImageInfo[] GetImageInfos()
        {
            var ret = new List<ImageInfo>();
            foreach (var imageName in imageNames)
            {
                ret.Add(new ImageInfo(imageName, () => packReader.Read(imageName)));
            }
            return [.. ret];
        }
    }
}
