using System;

namespace PPDFramework
{
    /// <summary>
    /// 処分可能なコンポーネントです。
    /// </summary>
    public abstract class DisposableComponent : IDisposable
    {
        bool disposed;

        /// <summary>
        /// 破棄されたかどうかを取得します。
        /// </summary>
        public bool Disposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// 処分する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを処分する
        /// </summary>
        protected virtual void DisposeResource()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    DisposeResource();
                }
            }
            disposed = true;
        }
    }
}
