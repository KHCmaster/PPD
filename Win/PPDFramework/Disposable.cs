using System;

namespace PPDFramework
{
    /// <summary>
    /// 処分可能なオブジェクトです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="disposedCallback"></param>
    public sealed class Disposable(Action disposedCallback) : IDisposable
    {
        bool disposed;
        readonly Action disposedCallback = disposedCallback;

        /// <summary>
        /// 処分する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && disposedCallback != null)
                {
                    disposedCallback();
                }
            }
            disposed = true;
        }
    }
}
