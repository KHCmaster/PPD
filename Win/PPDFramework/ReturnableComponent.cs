using System;

namespace PPDFramework
{
    /// <summary>
    /// 返却可能なオブジェクトです。
    /// </summary>
    public class ReturnableComponent : IDisposable
    {
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

        /// <summary>
        /// 破棄します。
        /// </summary>
        /// <param name="disposing">Dispose から呼ばれたときは true、ファイナライザーから呼ばれたときは false。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeResource();
            }
        }
    }
}
