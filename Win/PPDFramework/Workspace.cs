using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// ワークスペースのクラスです。
    /// </summary>
    public class Workspace : DisposableComponent
    {
        WorkspaceTexture primal;
        Stack<WorkspaceTexture> textures;
        HashSet<WorkspaceTexture> usingTextures;

        /// <summary>
        /// PPDデバイスを取得します。
        /// </summary>
        public PPDDevice Device
        {
            get;
            private set;
        }

        /// <summary>
        /// 1つの主要なテクスチャを取得します。
        /// </summary>
        public WorkspaceTexture Primal
        {
            get { return primal; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public Workspace(PPDDevice device)
        {
            Device = device;
            textures = new Stack<WorkspaceTexture>();
            primal = new WorkspaceTexture(device, this);
            for (var i = 0; i < 8; i++)
            {
                textures.Push(new WorkspaceTexture(device, this));
            }
            usingTextures = new HashSet<WorkspaceTexture>();
        }

        /// <summary>
        /// デバイスがロストしたときの処理です。
        /// </summary>
        public void OnLostDevice()
        {
            Primal.OnLostDevice();
            foreach (var texture in usingTextures)
            {
                texture.OnLostDevice();
            }
            foreach (var texture in textures)
            {
                texture.OnLostDevice();
            }
        }

        /// <summary>
        /// デバイスがリセットされたときの処理です。
        /// </summary>
        public void OnResetDevice()
        {
            Primal.OnResetDevice();
            foreach (var texture in usingTextures)
            {
                texture.OnResetDevice();
            }
            foreach (var texture in textures)
            {
                texture.OnResetDevice();
            }
        }

        /// <summary>
        /// リセットします。
        /// </summary>
        public void Reset()
        {
            foreach (var texture in usingTextures)
            {
                textures.Push(texture);
            }
            usingTextures.Clear();
        }

        /// <summary>
        /// ワークスペースのテクスチャを取得します。
        /// </summary>
        /// <returns></returns>
        public WorkspaceTexture Get()
        {
            lock (textures)
            {
                WorkspaceTexture ret;
                if (textures.Count == 0)
                {
                    ret = new WorkspaceTexture(Device, this);
                }
                else
                {
                    ret = textures.Pop();
                }
                ret.Use();
                usingTextures.Add(ret);
                return ret;
            }
        }

        /// <summary>
        /// ワークスペースのテクスチャの使用をやめます。
        /// </summary>
        /// <param name="texture"></param>
        public void Unuse(WorkspaceTexture texture)
        {
            lock (textures)
            {
                if (usingTextures.Remove(texture))
                {
                    textures.Push(texture);
                }
            }
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            while (textures.Count > 0)
            {
                textures.Pop().DisposeInternal();
            }
            foreach (var texture in usingTextures)
            {
                texture.DisposeInternal();
            }
            usingTextures.Clear();
            if (primal != null)
            {
                primal.Dispose();
                primal = null;
            }
        }
    }
}
