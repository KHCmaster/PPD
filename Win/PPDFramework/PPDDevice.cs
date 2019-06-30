using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// PPDのデバイスクラスです
    /// </summary>
    public abstract class PPDDevice : DisposableComponent
    {
        /// <summary>
        /// レンダーターゲットです。
        /// </summary>
        protected WorkspaceTexture renderTarget;

        /// <summary>
        /// モジュール一覧です。
        /// </summary>
        protected Dictionary<Type, DisposableComponent> modules;

        /// <summary>
        /// リセット可能なコンポーネント一覧です。
        /// </summary>
        protected HashSet<ResettableComponent> resettableComponents;

        /// <summary>
        /// 頂点定義を取得、設定します。
        /// </summary>
        internal abstract VertexDeclarationBase VertexDeclaration { get; set; }

        /// <summary>
        /// シェーダー名のプレフィックスを取得します。
        /// </summary>
        internal abstract string ShaderNamePrefix { get; }

        /// <summary>
        /// ワークスペースを取得します
        /// </summary>
        internal Workspace Workspace
        {
            get;
            private set;
        }

        /// <summary>
        /// オフセットを取得します
        /// </summary>
        public Vector2 Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// スケールを取得します
        /// </summary>
        public Vector2 Scale
        {
            get;
            private set;
        }

        /// <summary>
        /// プロジェクションを取得します
        /// </summary>
        public Matrix Projection
        {
            get;
            private set;
        }

        /// <summary>
        /// ワールドを取得します。
        /// </summary>
        public Matrix World
        {
            get;
            private set;
        }

        /// <summary>
        /// 幅を取得します
        /// </summary>
        public int Width
        {
            get;
            protected set;
        }

        /// <summary>
        /// 高さを取得します
        /// </summary>
        public int Height
        {
            get;
            protected set;
        }

        /// <summary>
        /// 期待する幅を取得します。
        /// </summary>
        public float ExpectedWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// 期待する高さを取得します。
        /// </summary>
        public float ExpectedHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// デバイスを取得します。
        /// </summary>
        public abstract ComObject Device
        {
            get;
        }

        internal PPDDevice(int width, int height, float expectedWidth, float expectedHeight, float expectedRatio)
        {
            Width = width;
            Height = height;
            ExpectedWidth = expectedWidth;
            ExpectedHeight = expectedHeight;
            var projection = Matrix.Identity;
            projection.M11 = 2.0f / width;
            projection.M22 = -2.0f / height;
            projection.M41 = -1;
            projection.M42 = 1;
            Projection = projection;
            float _width = width, _height = height;
            if (_width / _height > expectedRatio)
            {
                Scale = new Vector2((float)_height / expectedHeight);
                Offset = new Vector2((float)(_width - Scale.X * expectedWidth) / 2, 0);
            }
            else
            {
                Scale = new Vector2((float)_width / expectedWidth);
                Offset = new Vector2(0, (float)(_height - Scale.X * expectedHeight) / 2);
            }
            World = Matrix.AffineTransformation2D(Scale.X, Vector2.Zero, 0, Offset);
            modules = new Dictionary<Type, DisposableComponent>();
            resettableComponents = new HashSet<ResettableComponent>();
        }

        /// <summary>
        /// Workspaceを初期化します。
        /// </summary>
        internal void InitializeWorkspace()
        {
            if (Workspace != null)
            {
                Workspace.Dispose();
                Workspace = null;
            }
            Workspace = new Workspace(this);
        }

        /// <summary>
        /// リセットします
        /// </summary>
        internal void Reset()
        {
            Workspace.Reset();
            renderTarget = null;
        }

        /// <summary>
        /// 今のレンダーターゲットを取得します
        /// </summary>
        /// <returns></returns>
        internal WorkspaceTexture GetRenderTarget()
        {
            if (renderTarget == null)
            {
                renderTarget = Workspace.Get();
            }
            return renderTarget;
        }

        /// <summary>
        /// シェーダーのバイトコードを取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal byte[] GetShaderBytecode(string name)
        {
            var obj = PPDFramework.Properties.Resources.ResourceManager.GetObject(String.Format("{0}{1}", ShaderNamePrefix, name), PPDFramework.Properties.Resources.Culture);
            return ((byte[])(obj));
        }

        /// <summary>
        /// モジュールを登録します。
        /// </summary>
        /// <param name="module"></param>
        internal void RegisterModule(DisposableComponent module)
        {
            modules[module.GetType()] = module;
        }

        /// <summary>
        /// モジュールの登録を解除します。
        /// </summary>
        /// <param name="module"></param>
        internal void UnregisterModule(DisposableComponent module)
        {
            modules.Remove(module.GetType());
        }

        /// <summary>
        /// モジュールを取得します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : DisposableComponent
        {
            return (T)modules[typeof(T)];
        }

        /// <summary>
        /// 現在のレンダーターゲットを設定します。
        /// </summary>
        /// <param name="workSpaceTexture"></param>
        internal abstract void SetRenderTarget(WorkspaceTexture workSpaceTexture);

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        internal abstract void StretchRectangle(WorkspaceTexture src, WorkspaceTexture dest);

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sourceRect"></param>
        /// <param name="dest"></param>
        /// <param name="destPoint"></param>
        internal abstract void StretchRectangle(TextureBase src, RawRectangle? sourceRect, TextureBase dest, RawPoint? destPoint);

        /// <summary>
        /// クリアします。
        /// </summary>
        internal virtual void Clear()
        {
            Clear(PPDColors.Transparent);
        }

        /// <summary>
        /// クリアします。
        /// </summary>
        /// <param name="color"></param>
        internal abstract void Clear(Color4 color);

        /// <summary>
        /// デバイスをリセットします。
        /// </summary>
        internal abstract void ResetDevice();

        /// <summary>
        /// 描画を開始します。
        /// </summary>
        internal abstract void DrawStart();

        /// <summary>
        /// 描画を終了します。
        /// </summary>
        internal abstract void DrawEnd();

        /// <summary>
        /// 表示します。
        /// </summary>
        internal abstract void Present();

        /// <summary>
        /// ストリームを設定します。
        /// </summary>
        /// <param name="buffer"></param>
        internal abstract void SetStreamSource(VertexBufferBase buffer);

        /// <summary>
        /// インデックスを設定します。
        /// </summary>
        /// <param name="index"></param>
        internal abstract void SetIndices(IndexBufferBase index);

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="offset"></param>
        /// <param name="primitiveCount"></param>
        internal abstract void DrawPrimitives(PrimitiveType primitiveType, int offset, int primitiveCount);

        /// <summary>
        /// プリミティブを描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="baseVertexIndex"></param>
        /// <param name="minVertexIndex"></param>
        /// <param name="numVertices"></param>
        /// <param name="startIndex"></param>
        /// <param name="primCount"></param>
        internal abstract void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount);

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="offset"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="data"></param>
        internal abstract void DrawUserPrimitives<T>(PrimitiveType primitiveType, int offset, int primitiveCount, T[] data) where T : struct;

        /// <summary>
        /// バックバッファをファイルに書き込みます。
        /// </summary>
        /// <param name="filepath"></param>
        public abstract void BackBufferToFile(string filepath);

        /// <summary>
        /// 切り取りの有効状態を切り替えます。
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="enabled"></param>
        public abstract void SetScissorRect(Rectangle rectangle, bool enabled);

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (Workspace != null)
            {
                Workspace.Dispose();
                Workspace = null;
            }
            foreach (var module in modules.Values)
            {
                module.Dispose();
            }
            modules.Clear();
        }

        /// <summary>
        /// リセット可能なコンポーネントを追加します。
        /// </summary>
        /// <param name="resettable"></param>
        public void AddResettableComponent(ResettableComponent resettable)
        {
            lock (resettableComponents)
            {
                resettableComponents.Add(resettable);
            }
        }

        /// <summary>
        /// リセット可能なコンポーネントを削除します。
        /// </summary>
        /// <param name="resettable"></param>
        public void RemoveResettableComponent(ResettableComponent resettable)
        {
            lock (resettableComponents)
            {
                resettableComponents.Remove(resettable);
            }
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="expectedWidth"></param>
        /// <param name="expectedHeight"></param>
        /// <param name="expectedAspectRatio"></param>
        /// <param name="isDX11"></param>
        public static PPDDevice Initialize(IntPtr handle, int width, int height, int expectedWidth, int expectedHeight, float expectedAspectRatio, bool isDX11 = false)
        {
            PPDDevice ppdDevice;
            if (isDX11)
            {
                PPDSetting.Setting.ShaderDisabled = false;
                Effect.EffectFactoryManager.Initialize(new Effect.DX11.EffectFactory());
                Resource.ImageResourceFactoryManager.Initialize(new Resource.DX11.ImageResourceFactory());
                PPDFramework.Texture.TextureFactoryManager.Initialize(new Texture.DX11.TextureFactory());
                Vertex.VertexBufferFactoryManager.Initialize(new Vertex.DX11.VertexBufferFactory());
                Vertex.VertexDeclarationFactoryManager.Initialize(new Vertex.DX11.VertexDeclarationFactory());
                Vertex.IndexBufferFactoryManager.Initialize(new Vertex.DX11.IndexBufferFactory());
                ppdDevice = new DX11.PPDDevice(handle, width, height, expectedWidth, expectedHeight, expectedAspectRatio);
            }
            else
            {
                Effect.EffectFactoryManager.Initialize(new Effect.DX9.EffectFactory());
                Resource.ImageResourceFactoryManager.Initialize(new Resource.DX9.ImageResourceFactory());
                PPDFramework.Texture.TextureFactoryManager.Initialize(new Texture.DX9.TextureFactory());
                Vertex.VertexBufferFactoryManager.Initialize((IVertexBufferFactory)new Vertex.DX9.VertexBufferFactory());
                Vertex.VertexDeclarationFactoryManager.Initialize(new Vertex.DX9.VertexDeclarationFactory());
                Vertex.IndexBufferFactoryManager.Initialize(new Vertex.DX9.IndexBufferFactory());
                ppdDevice = new DX9.PPDDevice(handle, width, height, expectedWidth, expectedHeight, expectedAspectRatio);
            }
            ppdDevice.InitializeWorkspace();
            return ppdDevice;
        }
    }
}
