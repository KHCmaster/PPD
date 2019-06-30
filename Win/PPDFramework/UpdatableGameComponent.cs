using System;

namespace PPDFramework
{
    /// <summary>
    /// 更新可能なゲームコンポーネントクラス
    /// </summary>
    public abstract class UpdatableGameComponent : IUpdatable, IDisposable
    {
        /// <summary>
        /// 処分したか
        /// </summary>
        public bool disposed;

        /// <summary>
        /// 更新する
        /// </summary>
        public virtual void Update()
        {
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
