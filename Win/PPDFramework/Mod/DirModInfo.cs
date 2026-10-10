using System.IO;

namespace PPDFramework.Mod
{
    /// <summary>
    /// Modのフォルダ情報です。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="dirPath"></param>
    public class DirModInfo(string dirPath) : ModInfoBase(dirPath, true)
    {
        /// <summary>
        /// 表示名を取得します。
        /// </summary>
        public string DisplayName
        {
            get { return Path.GetFileNameWithoutExtension(ModPath); }
        }
    }
}
