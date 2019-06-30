using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// フォルダのスプライトマネージャークラスです。
    /// </summary>
    public class DirSpriteManager : SpriteManagerBase
    {
        string dir;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="dir"></param>
        public DirSpriteManager(string dir) : base(dir)
        {
            this.dir = dir;
        }

        /// <summary>
        /// 画像の情報一覧を取得します。
        /// </summary>
        /// <returns></returns>
        protected override ImageInfo[] GetImageInfos()
        {
            var infos = new List<ImageInfo>();
            foreach (var file in FindFiles(dir))
            {
                try
                {
                    infos.Add(new ImageInfo(file));
                }
                catch
                {
                }
            }
            return infos.ToArray();
        }

        /// <summary>
        /// 更新があるかどうかを確認します。
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected override bool CheckUpdate(ImageInfo[] infos)
        {
            foreach (var info in infos)
            {
                var dict = Dicts.FirstOrDefault(d => d.SpritesWithActualPath.ContainsKey(info.Path));
                if (dict == null || dict.SpritesWithActualPath[info.Path].LastWriteTime != info.LastWriteTime)
                {
                    return false;
                }
            }
            return true;
        }

        private IEnumerable<string> FindFiles(string dir)
        {
            foreach (var childDir in Directory.GetDirectories(dir))
            {
                foreach (var childFile in FindFiles(childDir))
                {
                    yield return childFile;
                }
            }
            foreach (var childFile in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(childFile).ToLower();
                if (Array.IndexOf(allowedExtensions, extension) >= 0)
                {
                    yield return childFile;
                }
            }
        }
    }
}
