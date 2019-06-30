using PPDFramework.Texture;

namespace PPDFramework
{
    /// <summary>
    /// ワークスペースのテクスチャクラスです。
    /// </summary>
    public class WorkspaceTexture : ReturnableComponent
    {
        /// <summary>
        /// ワークスペースを取得します。
        /// </summary>
        public Workspace Workspace
        {
            get;
            private set;
        }

        /// <summary>
        /// テクスチャを取得します。
        /// </summary>
        public TextureBase Texture
        {
            get;
            private set;
        }

        /// <summary>
        /// サーフェースを取得します。
        /// </summary>
        public SurfaceBase Surface
        {
            get { return Texture.Surface; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="workspace"></param>
        public WorkspaceTexture(PPDDevice device, Workspace workspace)
        {
            Workspace = workspace;
            Texture = TextureFactoryManager.Factory.CreateRenderTarget(device, device.Width, device.Height, 1, false);
        }

        /// <summary>
        /// デバイスがロストしたときの処理です。
        /// </summary>
        public void OnLostDevice()
        {
            Texture.Dispose();
            Texture = null;
        }

        /// <summary>
        /// デバイスがリセットされたときの処理です。
        /// </summary>
        public void OnResetDevice()
        {
            Texture = TextureFactoryManager.Factory.CreateRenderTarget(Workspace.Device, Workspace.Device.Width, Workspace.Device.Height, 1, false);
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
                if (Texture != null)
                {
                    Texture.Dispose();
                    Texture = null;
                }
            }
        }

        internal void DisposeInternal()
        {
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }
    }
}