using PPDFramework.Texture;

namespace PPDFramework
{
    /// <summary>
    /// ワークスペースのテクスチャクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    /// <param name="workspace"></param>
    public class WorkspaceTexture(PPDDevice device, Workspace workspace) : ReturnableComponent
    {
        /// <summary>
        /// ワークスペースを取得します。
        /// </summary>
        public Workspace Workspace
        {
            get;
            private set;
        } = workspace;

        /// <summary>
        /// テクスチャを取得します。
        /// </summary>
        public TextureBase Texture
        {
            get;
            private set;
        } = TextureFactoryManager.Factory.CreateRenderTarget(device, device.Width, device.Height, 1, false);

        /// <summary>
        /// サーフェースを取得します。
        /// </summary>
        public SurfaceBase Surface
        {
            get { return Texture.Surface; }
        }

        internal void Use()
        {
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (Workspace != null)
            {
                Workspace.Unuse(this);
            }
            else
            {
                Texture?.Dispose();
                Texture = null;
            }
        }

        internal void DisposeInternal()
        {
            Texture?.Dispose();
            Texture = null;
        }
    }
}