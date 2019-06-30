using System;

namespace PPDFramework
{
    /// <summary>
    /// 処分可能なオブジェクトです。
    /// </summary>
    public class Disposable : IDisposable
    {
        bool disposed;
        Action disposedCallback;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="disposedCallback"></param>
        public Disposable(Action disposedCallback)
        {
            this.disposedCallback = disposedCallback;
        }

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
