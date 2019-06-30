using PPDFramework.Sprites;
using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;

namespace PPDFramework.DX9
{
    /// <summary>
    /// DirectX9のPPDデバイスです。
    /// </summary>
    public class PPDDevice : PPDFramework.PPDDevice
    {
        Device device;
        Direct3D d3d;
        PresentParameters presentParam;
        Surface originalRenderTarget;
        VertexDeclarationBase vertexDeclaration;
        VertexBufferBase currentBuffer;
        IndexBufferBase currentIndex;
        Rectangle currentScissorRectangle;
        bool scissorRectEnabled;

        /// <summary>
        /// デバイスを取得します。
        /// </summary>
        public override ComObject Device
        {
            get { return device; }
        }

        /// <summary>
        /// Direct3Dを取得します。
        /// </summary>
        public Direct3D D3D
        {
            get { return d3d; }
        }

        /// <summary>
        /// Spriteを取得します。
        /// </summary>
        internal Sprite Sprite
        {
            get;
            private set;
        }

        internal SpriteBatch SpriteBatch
        {
            get;
            private set;
        }

        /// <summary>
        /// VertexDeclarationを取得、設定します。
        /// </summary>
        internal override VertexDeclarationBase VertexDeclaration
        {
            get { return vertexDeclaration; }
            set
            {
                if (vertexDeclaration != value)
                {
                    vertexDeclaration = value;
                    device.VertexDeclaration = ((Vertex.DX9.VertexDeclaration)vertexDeclaration)._VertexDeclaration;
                }
            }
        }

        /// <summary>
        /// シェーダープリフィックスを取得します。
        /// </summary>
        internal override string ShaderNamePrefix
        {
            get { return "d3d9_"; }
        }

        /// <summary>
        /// コンストラクタです
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="expectedWidth"></param>
        /// <param name="expectedHeight"></param>
        /// <param name="expectedRatio"></param>
        public PPDDevice(IntPtr handle, int width, int height, float expectedWidth, float expectedHeight, float expectedRatio) :
            base(width, height, expectedWidth, expectedHeight, expectedRatio)
        {
            d3d = new Direct3D();
            presentParam = new PresentParameters
            {
                Windowed = true,
                BackBufferHeight = height,
                BackBufferWidth = width,
                BackBufferFormat = d3d.Adapters[0].CurrentDisplayMode.Format,
                BackBufferCount = 1,
                SwapEffect = SwapEffect.Discard,
                PresentationInterval = PresentInterval.Default
            };

            // TODO
            //presentParam.Multisample = PPDSetting.Setting.MultiSample;
            var createFlag = CreateFlags.Multithreaded | CreateFlags.FpuPreserve;
            try
            {
                device = new Device(d3d, 0, DeviceType.Hardware, handle, CreateFlags.HardwareVertexProcessing | createFlag, presentParam);
            }
            catch (SharpDXException)
            {
                try
                {
                    device = new Device(d3d, 0, DeviceType.Hardware, handle, CreateFlags.SoftwareVertexProcessing | createFlag, presentParam);
                }
                catch (SharpDXException)
                {
                    try
                    {
                        device = new Device(d3d, 0, DeviceType.Reference, handle, CreateFlags.SoftwareVertexProcessing | createFlag, presentParam);
                    }
                    catch (SharpDXException)
                    {
                        throw;
                    }
                }
            }

            if (PPDSetting.Setting.ShaderDisabled)
            {
                Sprite = new Sprite(device);
            }
            else
            {
                SpriteBatch = new SpriteBatch(this);
            }
        }

        /// <summary>
        /// 現在のレンダーターゲットを設定します。
        /// </summary>
        /// <param name="workSpaceTexture"></param>
        internal override void SetRenderTarget(WorkspaceTexture workSpaceTexture)
        {
            renderTarget = workSpaceTexture;
            device.SetRenderTarget(0, ((Texture.DX9.Surface)workSpaceTexture.Surface)._Surface);
            SetScissorRectImpl();
        }

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        internal override void StretchRectangle(WorkspaceTexture src, WorkspaceTexture dest)
        {
            if (PPDSetting.Setting.ShaderDisabled)
            {
                return;
            }
            device.StretchRectangle(((Texture.DX9.Surface)src.Surface)._Surface, ((Texture.DX9.Surface)dest.Surface)._Surface, TextureFilter.Point);
        }

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sourceRect"></param>
        /// <param name="dest"></param>
        /// <param name="destPoint"></param>
        internal override void StretchRectangle(TextureBase src, RawRectangle? sourceRect, TextureBase dest, RawPoint? destPoint)
        {
            device.UpdateSurface(
                ((Texture.DX9.Surface)src.Surface)._Surface,
                sourceRect,
                ((Texture.DX9.Surface)dest.Surface)._Surface,
                destPoint);
        }

        /// <summary>
        /// クリアします。
        /// </summary>
        /// <param name="color"></param>
        internal override void Clear(Color4 color)
        {
            device.SetRenderState(RenderState.ScissorTestEnable, false);
            device.Clear(ClearFlags.Target, new ColorBGRA(color), 1, 0);
            SetScissorRectImpl();
        }

        /// <summary>
        /// デバイスをリセットします。
        /// </summary>
        internal override void ResetDevice()
        {
            var r = device.TestCooperativeLevel();
            if (r.Code != ResultCode.Success.Code)
            {
                if (r.Code == ResultCode.DeviceLost.Code)
                {
                    System.Threading.Thread.Sleep(10);
                }
                else if (r.Code == ResultCode.DeviceNotReset.Code)
                {
                    try
                    {
                        vertexDeclaration = null;
                        if (originalRenderTarget != null && !originalRenderTarget.IsDisposed)
                        {
                            device.SetRenderTarget(0, originalRenderTarget);
                            originalRenderTarget.Dispose();
                        }
                        Workspace.OnLostDevice();
                        foreach (var resettable in resettableComponents)
                        {
                            resettable.OnLostDevice();
                        }
                        device.Reset(presentParam);
                        foreach (var resettable in resettableComponents)
                        {
                            resettable.OnResetDevice();
                        }
                        Workspace.OnResetDevice();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to reset device", e);
                    }
                }
            }
        }

        /// <summary>
        /// 描画を開始します。
        /// </summary>
        internal override void DrawStart()
        {
            currentBuffer = null;
            originalRenderTarget = device.GetRenderTarget(0);
            Reset();
            var newRenderTarget = GetRenderTarget();
            SetRenderTarget(newRenderTarget);
            device.Clear(ClearFlags.Target, new ColorBGRA(0f, 0f, 0f, 1f), 1.0f, 0);
            SetScissorRect(new Rectangle(0, 0, Width, Height), false);
            device.BeginScene();
            if (Sprite != null)
            {
                Sprite.Begin(SpriteFlags.AlphaBlend);
            }
        }

        /// <summary>
        /// 描画を終了します。
        /// </summary>
        internal override void DrawEnd()
        {
            if (Sprite != null)
            {
                Sprite.End();
            }
            if (SpriteBatch != null)
            {
                SpriteBatch.Flush();
                SpriteBatch.End();
            }
            device.SetRenderTarget(0, originalRenderTarget);
            device.StretchRectangle(((Texture.DX9.Surface)GetRenderTarget().Surface)._Surface, originalRenderTarget, TextureFilter.Point);
            originalRenderTarget.Dispose();
            device.EndScene();
        }

        /// <summary>
        /// 表示します。
        /// </summary>
        internal override void Present()
        {
            device.Present();
        }

        /// <summary>
        /// プリミティブを描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="offset"></param>
        /// <param name="primitiveCount"></param>
        internal override void DrawPrimitives(PrimitiveType primitiveType, int offset, int primitiveCount)
        {
            device.DrawPrimitives((SharpDX.Direct3D9.PrimitiveType)primitiveType, offset, primitiveCount);
        }

        /// <summary>
        /// プリミティブを描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="baseVertexIndex"></param>
        /// <param name="minVertexIndex"></param>
        /// <param name="numVertices"></param>
        /// <param name="startIndex"></param>
        /// <param name="primCount"></param>
        internal override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount)
        {
            device.DrawIndexedPrimitive((SharpDX.Direct3D9.PrimitiveType)primitiveType, baseVertexIndex, minVertexIndex, numVertices, startIndex, primCount);
        }

        /// <summary>
        /// ユーザープリミティブを描画します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primitiveType"></param>
        /// <param name="offset"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="data"></param>
        internal override void DrawUserPrimitives<T>(PrimitiveType primitiveType, int offset, int primitiveCount, T[] data)
        {
            device.DrawUserPrimitives((SharpDX.Direct3D9.PrimitiveType)primitiveType, offset, primitiveCount, data);
        }

        /// <summary>
        /// ストリームソースを設定します。
        /// </summary>
        /// <param name="buffer"></param>
        internal override void SetStreamSource(VertexBufferBase buffer)
        {
            if (currentBuffer != buffer)
            {
                device.SetStreamSource(0, ((Vertex.DX9.VertexBuffer)buffer)._VertexBuffer, 0, ColoredTexturedVertex.Size);
                currentBuffer = buffer;
            }
        }

        /// <summary>
        /// インデックスを設定します。
        /// </summary>
        /// <param name="index"></param>
        internal override void SetIndices(IndexBufferBase index)
        {
            if (currentIndex != index)
            {
                device.Indices = ((Vertex.DX9.IndexBuffer)index)._IndexBuffer;
                currentIndex = index;
            }
        }

        /// <summary>
        /// バックバッファーをファイルに書き込みます。
        /// </summary>
        /// <param name="filepath"></param>
        public override void BackBufferToFile(string filepath)
        {
            var surface = device.GetBackBuffer(0, 0);
            Surface.ToFile(surface, filepath, ImageFileFormat.Png);
        }

        /// <summary>
        /// シザーレクトを設定します。
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="enabled"></param>
        public override void SetScissorRect(Rectangle rectangle, bool enabled)
        {
            if (SpriteBatch != null)
            {
                SpriteBatch.SetScissorRect(rectangle, enabled);
            }
            else
            {
                currentScissorRectangle = rectangle;
                scissorRectEnabled = enabled;
                SetScissorRectImpl();
            }
        }

        private void SetScissorRectImpl()
        {
            device.ScissorRect = currentScissorRectangle;
            device.SetRenderState(RenderState.ScissorTestEnable, scissorRectEnabled);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (d3d != null)
            {
                d3d.Dispose();
                d3d = null;
            }
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
            if (Sprite != null)
            {
                Sprite.Dispose();
                Sprite = null;
            }
        }
    }
}
