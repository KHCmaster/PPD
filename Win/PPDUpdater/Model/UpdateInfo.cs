using System.Collections.Generic;

namespace PPDUpdater.Model
{
    class UpdateInfo
    {
        public VersionInfo VersionInfo
        {
            get;
            private set;
        }

        public string UrlPath
        {
            get;
            private set;
        }

        public AssemblyType AssemblyType
        {
            get;
            private set;
        }

        public string FilePath
        {
            get;
            set;
        }

        public UpdateInfo(string version, string urlpath)
        {
            VersionInfo = new VersionInfo(version);
            UrlPath = urlpath;
            AssemblyType = AssemblyType.x64;
        }

        public override string ToString()
        {
            return VersionInfo.ToString();
        }
    }

    class UpdateInfoComparer : IComparer<UpdateInfo>
    {
        public static UpdateInfoComparer Comparer
        {
            get;
            private set;
        }

        static UpdateInfoComparer()
        {
            Comparer = new UpdateInfoComparer();
        }

        #region IComparer<UpdateInfo> メンバ

        public int Compare(UpdateInfo x, UpdateInfo y)
        {
            return VersionInfoComparer.Comparer.Compare(x.VersionInfo, y.VersionInfo);
        }

        #endregion
    }
}
