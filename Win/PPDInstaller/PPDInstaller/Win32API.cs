using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PPDInstaller
{
    class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        const int CSIDL_COMMON_PROGRAMS = 0x17;

        public static string GetCommonStartmenuPath()
        {
            var path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_PROGRAMS, false);
            return path.ToString();
        }

        [DllImport("kernel32.dll")]
        internal static extern int GetACP();
    }
}
