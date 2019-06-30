using PPDPack;
using System.Collections.Generic;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// PPDPackのスプライトマネージャークラスです。
    /// </summary>
    public abstract class PPDPackSpriteManagerBase : SpriteManagerBase
    {
        string[] imageNames;
        PackReader packReader;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="packReader"></param>
        /// <param name="imageNames"></param>
        /// <param name="spriteDirName"></param>
        protected PPDPackSpriteManagerBase(PackReader packReader, string[] imageNames, string spriteDirName) : base(spriteDirName)
        {
            this.packReader = packReader;
            this.imageNames = imageNames;
        }

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
            return ret.ToArray();
        }
    }
}
