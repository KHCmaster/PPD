using System;
using System.IO;

namespace PPDFramework.Texture
{
    /// <summary>
    /// アンマネージドメモリを読み書きするストリームです（SharpDX の DataStream の代わり）。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="dataPointer"></param>
    /// <param name="length"></param>
    /// <param name="canRead"></param>
    /// <param name="canWrite"></param>
    public unsafe class DataStream(IntPtr dataPointer, long length, bool canRead, bool canWrite) : UnmanagedMemoryStream((byte*)dataPointer, length, length, canWrite ? (canRead ? FileAccess.ReadWrite : FileAccess.Write) : FileAccess.Read)
    {
        /// <summary>
        /// データの先頭のポインターを取得します。
        /// </summary>
        public IntPtr DataPointer
        {
            get;
            private set;
        } = dataPointer;
    }
}
