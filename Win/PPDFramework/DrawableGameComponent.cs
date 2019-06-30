using SharpDX;
using System;

namespace PPDFramework
{
    /// <summary>
    /// 描画可能なゲームコンポーネント
    /// </summary>
    public abstract class DrawableGameComponent : IDrawable, IDisposable
    {
        /// <summary>
        /// 処分したか
        /// </summary>
        public bool disposed;

        /// <summary>
        /// 描画する
        /// </summary>
        public virtual void Draw()
        {
        }

        /// <summary>
        /// 位置
        /// </summary>
        public virtual Vector2 Position { get; set; }

        /// <summary>
        /// アルファ
        /// </summary>
        public virtual float Alpha { get; set; }

        /// <summary>
        /// ヒドゥン
        /// </summary>
        public virtual bool Hidden { get; set; }

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
