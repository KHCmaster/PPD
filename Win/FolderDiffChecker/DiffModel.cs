using System;

namespace FolderDiffChecker
{
    /// <summary>
    /// 見つかった差分 1 つです。
    /// </summary>
    public class DiffModel
    {
        [Flags]
        public enum DiffMode
        {
            Add,
            Edit
        }

        public DiffMode Mode
        {
            get;
            set;
        }

        public bool IsFile
        {
            get;
            set;
        }

        public string FilePath
        {
            get;
            set;
        }
    }
}
