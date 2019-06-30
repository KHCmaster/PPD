using System.Collections.Generic;

namespace PPDFramework.Mod
{
    /// <summary>
    /// Modの情報の基底クラスです。
    /// </summary>
    public abstract class ModInfoBase
    {
        private List<ModInfoBase> childModInfos;

        /// <summary>
        /// 親を取得します。
        /// </summary>
        public ModInfoBase Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// 子の一覧を取得します。
        /// </summary>
        public ModInfoBase[] Children
        {
            get { return childModInfos.ToArray(); }
        }

        /// <summary>
        /// フォルダかどうかを取得します。
        /// </summary>
        public bool IsDir
        {
            get;
            private set;
        }

        /// <summary>
        /// パスを取得します。
        /// </summary>
        public string ModPath
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        protected ModInfoBase(string path, bool isDir)
        {
            childModInfos = new List<ModInfoBase>();
            IsDir = isDir;
            ModPath = path;
        }

        /// <summary>
        /// 子を追加します。
        /// </summary>
        /// <param name="childInfo"></param>
        public void AddChild(ModInfoBase childInfo)
        {
            childInfo.Parent = this;
            childModInfos.Add(childInfo);
        }

        /// <summary>
        /// 子孫を全て取得します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModInfoBase> Descendants()
        {
            foreach (var child in childModInfos)
            {
                yield return child;
                foreach (var childChild in child.Descendants())
                {
                    yield return childChild;
                }
            }
        }
    }
}
