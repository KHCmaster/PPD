using System;

namespace PPDFramework
{
    /// <summary>
    /// 返却可能なオブジェクトです。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "派生クラスは DisposeResource をオーバーライドして破棄する設計")]
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

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeResource();
            }
        }
    }
}
