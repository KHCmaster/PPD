using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// ワークスペースのクラスです。
    /// </summary>
    public class Workspace : DisposableComponent
    {
        WorkspaceTexture primal;
        readonly Stack<WorkspaceTexture> textures;
        readonly HashSet<WorkspaceTexture> usingTextures;

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
            usingTextures = [];
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
                WorkspaceTexture.Use();
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
            primal?.DisposeInternal();
            primal = null;
        }
    }
}
