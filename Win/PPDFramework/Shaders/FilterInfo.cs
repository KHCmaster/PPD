using SharpDX;
using System.Runtime.InteropServices;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// フィルターの情報クラスです。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FilterInfo
    {
        /// <summary>
        /// １つ目の引数です。
        /// </summary>
        public Vector4 Arg1;

        /// <summary>
        /// ２つ目の引数です。
        /// </summary>
        public Vector4 Arg2;

        /// <summary>
        /// ３つ目の引数です。
        /// </summary>
        public Vector4 Arg3;
    }
}
