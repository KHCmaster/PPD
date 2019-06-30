using System;

namespace FolderDiffChecker
{
    class DiffModel
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
