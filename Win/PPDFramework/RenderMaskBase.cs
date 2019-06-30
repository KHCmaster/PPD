namespace PPDFramework
{
    /// <summary>
    /// レンダリング時のマスクです。
    /// </summary>
    public abstract class RenderMaskBase
    {
        /// <summary>
        /// 有効かどうかを取得します。
        /// </summary>
        public abstract bool Enabled { get; }

        /// <summary>
        /// 優先度を取得します。
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="maskTexture"></param>
        public abstract void Draw(PPDDevice device, WorkspaceTexture maskTexture);
    }
}
