using System;
using System.Collections.Generic;

namespace PPDUpdater.Model
{
    public class VersionInfo : IComparable<VersionInfo>
    {
        public int Major
        {
            get;
            private set;
        }

        public int Minor
        {
            get;
            private set;
        }

        public int Build
        {
            get;
            private set;
        }

        public int Revision
        {
            get;
            private set;
        }

        public VersionInfo(string version)
        {
            var data = version.Split('.');
            if (data.Length == 1 && int.TryParse(data[0], out int test))
            {
                if (version.Length >= 1) Major = int.Parse(version[0].ToString());
                if (version.Length >= 2) Minor = int.Parse(version[1].ToString());
                if (version.Length >= 3) Build = int.Parse(version[2].ToString());
                if (version.Length >= 4) Revision = int.Parse(version[3].ToString());
            }
            else
            {
                if (data.Length >= 1)
                {
                    if (int.TryParse(data[0], out int num))
                    {
                        Major = num;
                    }
                }
                if (data.Length >= 2)
                {
                    if (int.TryParse(data[1], out int num))
                    {
                        Minor = num;
                    }
                }
                if (data.Length >= 3)
                {
                    if (int.TryParse(data[2], out int num))
                    {
                        Build = num;
                    }
                }
                if (data.Length >= 4)
                {
                    if (int.TryParse(data[3], out int num))
                    {
                        Revision = num;
                    }
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
        }

        public static VersionInfo Zero = new VersionInfo("0.0.0.0");

        #region IComparable<VersionInfo> メンバ

        public int CompareTo(VersionInfo other)
        {
            if (other == null)
            {
                return 1;
            }

            if (this.Major == other.Major)
            {
                if (this.Minor == other.Minor)
                {
                    if (this.Build == other.Build)
                    {
                        if (this.Revision == other.Revision)
                        {
                            return 0;
                        }
                        else
                        {
                            return this.Revision - other.Revision;
                        }
                    }
                    else
                    {
                        return this.Build - other.Build;
                    }
                }
                else
                {
                    return this.Minor - other.Minor;
                }
            }
            else
            {
                return this.Major - other.Major;
            }
        }

        #endregion
    }

    class VersionInfoComparer : IComparer<VersionInfo>
    {
        public static VersionInfoComparer Comparer
        {
            get;
            private set;
        }

        static VersionInfoComparer()
        {
            Comparer = new VersionInfoComparer();
        }


        #region IComparer<VersionInfo> メンバ

        public int Compare(VersionInfo x, VersionInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            if (x.Major == y.Major)
            {
                if (x.Minor == y.Minor)
                {
                    if (x.Build == y.Build)
                    {
                        if (x.Revision == y.Revision)
                        {
                            return 0;
                        }
                        else
                        {
                            return x.Revision - y.Revision;
                        }
                    }
                    else
                    {
                        return x.Build - y.Build;
                    }
                }
                else
                {
                    return x.Minor - y.Minor;
                }
            }
            else
            {
                return x.Major - y.Major;
            }
        }

        #endregion
    }
}
