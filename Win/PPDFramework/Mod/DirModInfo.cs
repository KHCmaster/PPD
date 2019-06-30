using System.IO;

namespace PPDFramework.Mod
{
    /// <summary>
    /// Modのフォルダ情報です。
    /// </summary>
    public class DirModInfo : ModInfoBase
    {
        /// <summary>
        /// 表示名を取得します。
        /// </summary>
        public string DisplayName
        {
            get { return Path.GetFileNameWithoutExtension(ModPath); }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="dirPath"></param>
        public DirModInfo(string dirPath)
            : base(dirPath, true)
        {
        }
    }
}
