using PPDFramework.Sprites;
using PPDFramework.Texture;
using PPDFramework.Vertex;
using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;
using Rectangle = Vortice.Mathematics.RectI;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PPDFramework.DX11
{
    /// <summary>
    /// DirectX11のPPDデバイスです。
    /// </summary>
    class PPDDevice : PPDFramework.PPDDevice
    {
        const int MaxFanPrimitiveCount = 4096;

        ID3D11Device device;
        IDXGISwapChain swapChain;
        ID3D11Texture2D backBuffer;
        VertexDeclarationBase vertexDeclaration;
        VertexBufferBase currentBuffer;
        IndexBufferBase currentIndex;
        ID3D11RasterizerState scissorEnabledState;
        ID3D11RasterizerState scissorDisabledState;
        ID3D11Buffer fanIndexBuffer;
        Dictionary<string, ID3D11BlendState> blendStates;
        readonly Dictionary<PrimitiveType, PrimitiveTopology> primitiveTypeMap = new()
        {
            { PrimitiveType.PointList, PrimitiveTopology.PointList },
            { PrimitiveType.LineList, PrimitiveTopology.LineList },
            { PrimitiveType.LineStrip, PrimitiveTopology.LineStrip },
            { PrimitiveType.TriangleList, PrimitiveTopology.TriangleList },
            { PrimitiveType.TriangleStrip, PrimitiveTopology.TriangleStrip },
            { PrimitiveType.TriangleFan, PrimitiveTopology.TriangleList }
        };

        /// <summary>
        /// デバイスを取得します。
        /// </summary>
        public override ComObject Device
        {
            get { return device; }
        }

        /// <summary>
        /// デバイスコンテキストを取得します。
        /// </summary>
        public ID3D11DeviceContext Context
        {
            get;
            private set;
        }

        /// <summary>
        /// シェーダーで使うサンプラーを取得します（Common.fx の LinearSampler）。
        /// </summary>
        internal ID3D11SamplerState LinearSampler
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
                    Context.IASetInputLayout(((Vertex.DX11.VertexDeclaration)vertexDeclaration).InputLayout);
                }
            }
        }

        /// <summary>
        /// シェーダープリフィックスを取得します。
        /// </summary>
        internal override string ShaderNamePrefix
        {
            get { return "d3d11_"; }
        }

        /// <summary>
        /// 使用しているDirectXのバージョン名を取得します。
        /// </summary>
        public override string DirectXVersion
        {
            get { return "DirectX 11"; }
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
            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                BufferDescription = new ModeDescription((uint)width, (uint)height, new Rational(60, 1), Texture.DX11.TextureFactory.TextureFormat),
                Windowed = true,
                OutputWindow = handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                BufferUsage = Usage.RenderTargetOutput
            };
            var featureLevels = new[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };
            var flag = DeviceCreationFlags.BgraSupport;
            if (ComDiagnostics.IsEnabled)
            {
                flag |= DeviceCreationFlags.Debug;
            }
            try
            {
                CreateDeviceAndSwapChain(DriverType.Hardware, flag, featureLevels, swapChainDescription);
            }
            catch (SharpGenException)
            {
                CreateDeviceAndSwapChain(DriverType.Warp, flag, featureLevels, swapChainDescription);
            }
            // フルスクリーンの切り替えはアプリケーション側で行うので、Alt+Enterを無効にします
            using (var factory = swapChain.GetParent<IDXGIFactory>())
            {
                factory.MakeWindowAssociation(handle, WindowAssociationFlags.IgnoreAll).CheckError();
            }
            backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
            Context.RSSetViewport(0, 0, width, height, 0.0f, 1.0f);
            scissorDisabledState = device.CreateRasterizerState(new RasterizerDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                DepthClipEnable = true,
                ScissorEnable = false
            });
            scissorEnabledState = device.CreateRasterizerState(new RasterizerDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                DepthClipEnable = true,
                ScissorEnable = true
            });
            Context.RSSetState(scissorDisabledState);
            CreateShaderStates();
            CreateFanIndexBuffer();
            SpriteBatch = new SpriteBatch(this);
        }

        private void CreateDeviceAndSwapChain(DriverType driverType, DeviceCreationFlags flags, FeatureLevel[] featureLevels, SwapChainDescription swapChainDescription)
        {
            D3D11.D3D11CreateDeviceAndSwapChain(null, driverType, flags, featureLevels, swapChainDescription,
                out IDXGISwapChain newSwapChain, out ID3D11Device newDevice, out _, out ID3D11DeviceContext newContext).CheckError();
            swapChain = newSwapChain;
            device = newDevice;
            Context = newContext;
        }

        /// <summary>
        /// Common.fx で定義しているブレンドステートとサンプラーを作成します。
        /// </summary>
        private void CreateShaderStates()
        {
            LinearSampler = device.CreateSamplerState(new SamplerDescription
            {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunc = ComparisonFunction.Never,
                MaxAnisotropy = 1,
                MinLOD = float.MinValue,
                MaxLOD = float.MaxValue
            });
            blendStates = new Dictionary<string, ID3D11BlendState>
            {
                { "NoBlend", CreateBlendState(false, Blend.One, Blend.Zero) },
                { "AlphaBlending", CreateBlendState(true, Blend.SourceAlpha, Blend.InverseSourceAlpha) },
                { "PremultipliedAlphaBlending", CreateBlendState(true, Blend.One, Blend.InverseSourceAlpha) }
            };
        }

        private ID3D11BlendState CreateBlendState(bool enabled, Blend source, Blend destination)
        {
            var description = new BlendDescription();
            description.RenderTarget[0].BlendEnable = enabled;
            description.RenderTarget[0].SourceBlend = source;
            description.RenderTarget[0].DestinationBlend = destination;
            description.RenderTarget[0].BlendOperation = BlendOperation.Add;
            description.RenderTarget[0].SourceBlendAlpha = source;
            description.RenderTarget[0].DestinationBlendAlpha = destination;
            description.RenderTarget[0].BlendOperationAlpha = BlendOperation.Add;
            description.RenderTarget[0].RenderTargetWriteMask = ColorWriteEnable.All;
            return device.CreateBlendState(description);
        }

        /// <summary>
        /// 名前からブレンドステートを取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal ID3D11BlendState GetBlendState(string name)
        {
            if (!blendStates.TryGetValue(name, out ID3D11BlendState blendState))
            {
                throw new ArgumentException("Unknown blend state: " + name);
            }
            return blendState;
        }

        private void CreateFanIndexBuffer()
        {
            var indices = new int[MaxFanPrimitiveCount * 3];
            for (var i = 0; i < MaxFanPrimitiveCount; i++)
            {
                indices[i * 3] = 0;
                indices[i * 3 + 1] = i + 1;
                indices[i * 3 + 2] = i + 2;
            }
            fanIndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer, ResourceUsage.Immutable, CpuAccessFlags.None, ResourceOptionFlags.None, 0, 0);
        }

        /// <summary>
        /// 現在のレンダーターゲットを設定します。
        /// </summary>
        /// <param name="workSpaceTexture"></param>
        internal override void SetRenderTarget(WorkspaceTexture workSpaceTexture)
        {
            renderTarget = workSpaceTexture;
            Context.OMSetRenderTargets(((Texture.DX11.Texture)workSpaceTexture.Texture).RenderTargetView);
        }

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        internal override void StretchRectangle(WorkspaceTexture src, WorkspaceTexture dest)
        {
            Context.CopyResource(((Texture.DX11.Texture)dest.Texture)._Texture, ((Texture.DX11.Texture)src.Texture)._Texture);
        }

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sourceRect"></param>
        /// <param name="dest"></param>
        /// <param name="destPoint"></param>
        internal override void StretchRectangle(TextureBase src, Vortice.RawRect? sourceRect, TextureBase dest, Vortice.Mathematics.Int2? destPoint)
        {
            ((Texture.DX11.Texture)dest).CopyFrom((Texture.DX11.Texture)src, sourceRect, destPoint);
        }

        /// <summary>
        /// クリアします。
        /// </summary>
        /// <param name="color"></param>
        internal override void Clear(Color4 color)
        {
            Context.ClearRenderTargetView(((Texture.DX11.Texture)GetRenderTarget().Texture).RenderTargetView, color);
        }

        /// <summary>
        /// 描画を開始します。
        /// </summary>
        internal override void DrawStart()
        {
            currentBuffer = null;
            currentIndex = null;
            Reset();
            var newRenderTarget = GetRenderTarget();
            SetRenderTarget(newRenderTarget);
            Context.RSSetViewport(0, 0, Width, Height, 0.0f, 1.0f);
            Context.ClearRenderTargetView(((Texture.DX11.Texture)newRenderTarget.Texture).RenderTargetView, new Color4(0f, 0f, 0f, 1f));
            SetScissorRect(new Rectangle(0, 0, Width, Height), false);
        }

        /// <summary>
        /// 描画を終了します。
        /// </summary>
        internal override void DrawEnd()
        {
            SpriteBatch.Flush();
            SpriteBatch.End();
            Context.UnsetRenderTargets();
            Context.CopyResource(backBuffer, ((Texture.DX11.Texture)GetRenderTarget().Texture)._Texture);
        }

        /// <summary>
        /// 表示します。
        /// </summary>
        internal override void Present()
        {
            swapChain.Present(1, PresentFlags.None).CheckError();
        }

        /// <summary>
        /// プリミティブを描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="offset"></param>
        /// <param name="primitiveCount"></param>
        internal override void DrawPrimitives(PrimitiveType primitiveType, int offset, int primitiveCount)
        {
            Context.IASetPrimitiveTopology(primitiveTypeMap[primitiveType]);
            if (primitiveType == PrimitiveType.TriangleFan)
            {
                DrawFan(offset, primitiveCount);
                return;
            }
            Context.Draw((uint)GetVertexCount(primitiveType, primitiveCount), (uint)offset);
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
            if (primitiveType == PrimitiveType.TriangleFan)
            {
                throw new NotSupportedException("TriangleFan is not supported for indexed drawing in DirectX11");
            }
            Context.IASetPrimitiveTopology(primitiveTypeMap[primitiveType]);
            Context.DrawIndexed((uint)GetVertexCount(primitiveType, primCount), (uint)startIndex, baseVertexIndex);
        }


        private void DrawFan(int offset, int primitiveCount)
        {
            if (primitiveCount > MaxFanPrimitiveCount)
            {
                throw new NotSupportedException("Too many primitives for TriangleFan");
            }
            Context.IASetIndexBuffer(fanIndexBuffer, Format.R32_UInt, 0);
            currentIndex = null;
            Context.DrawIndexed((uint)(primitiveCount * 3), 0, offset);
        }

        private static int GetVertexCount(PrimitiveType primitiveType, int primitiveCount)
        {
            return primitiveType switch
            {
                PrimitiveType.PointList => primitiveCount,
                PrimitiveType.LineList => primitiveCount * 2,
                PrimitiveType.LineStrip => primitiveCount + 1,
                PrimitiveType.TriangleList => primitiveCount * 3,
                _ => primitiveCount + 2,
            };
        }

        /// <summary>
        /// ストリームソースを設定します。
        /// </summary>
        /// <param name="buffer"></param>
        internal override void SetStreamSource(VertexBufferBase buffer)
        {
            var dx11Buffer = (Vertex.DX11.VertexBuffer)buffer;
            dx11Buffer.Flush(Context);
            if (currentBuffer == buffer)
            {
                return;
            }
            currentBuffer = buffer;
            Context.IASetVertexBuffer(0, dx11Buffer.Buffer, (uint)ColoredTexturedVertex.Size, 0);
        }

        /// <summary>
        /// インデックスを設定します。
        /// </summary>
        /// <param name="index"></param>
        internal override void SetIndices(IndexBufferBase index)
        {
            if (currentIndex == index)
            {
                return;
            }
            currentIndex = index;
            Context.IASetIndexBuffer(((Vertex.DX11.IndexBuffer)index).Buffer, Format.R32_UInt, 0);
        }

        /// <summary>
        /// バックバッファーをファイルに書き込みます。
        /// </summary>
        /// <param name="filepath"></param>
        public override void BackBufferToFile(string filepath)
        {
            var pixels = Texture.DX11.Texture.ReadPixels(Context, backBuffer);
            using var bitmap = Texture.DX11.Texture.ToBitmap(pixels, (int)backBuffer.Description.Width, (int)backBuffer.Description.Height);
            bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
        }

        /// <summary>
        /// シザーレクトを設定します。
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="enabled"></param>
        public override void SetScissorRect(Rectangle rectangle, bool enabled)
        {
            SpriteBatch.SetScissorRect(rectangle, enabled);
        }

        /// <summary>
        /// 切り取りをすぐにデバイスに反映します。
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="enabled"></param>
        internal override void ApplyScissorRect(Rectangle rectangle, bool enabled)
        {
            Context.RSSetScissorRect(new Vortice.RawRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
            Context.RSSetState(enabled ? scissorEnabledState : scissorDisabledState);
        }

        /// <summary>
        /// スプライト描画用のアルファブレンドの状態を設定します。
        /// DirectX11ではエフェクトのパスで設定されるので何もしません。
        /// </summary>
        /// <param name="pa"></param>
        internal override void SetSpriteAlphaBlend(bool pa)
        {
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (Context != null)
            {
                Context.ClearState();
                Context.Flush();
            }

            fanIndexBuffer?.Dispose();
            fanIndexBuffer = null;
            if (blendStates != null)
            {
                foreach (var blendState in blendStates.Values)
                {
                    blendState.Dispose();
                }
                blendStates = null;
            }

            LinearSampler?.Dispose();
            LinearSampler = null;
            scissorEnabledState?.Dispose();
            scissorEnabledState = null;
            scissorDisabledState?.Dispose();
            scissorDisabledState = null;
            backBuffer?.Dispose();
            backBuffer = null;
            swapChain?.Dispose();
            swapChain = null;
            Context?.Dispose();
            Context = null;
            ReportLiveDeviceObjects();
            ComDiagnostics.ReportTrackedObjects("Textures not disposed");
            device?.Dispose();
            device = null;
        }

        /// <summary>
        /// デバイスに残っているオブジェクトを書き出します（<see cref="ComDiagnostics"/> が有効なときだけ）。
        /// デバイス自身と、ここで取得するデバッグ用のインターフェース以外が残っていれば解放漏れです。
        /// </summary>
        private void ReportLiveDeviceObjects()
        {
            if (!ComDiagnostics.IsEnabled || device == null)
            {
                return;
            }
            var debug = device.QueryInterfaceOrNull<Vortice.Direct3D11.Debug.ID3D11Debug>();
            var infoQueue = device.QueryInterfaceOrNull<Vortice.Direct3D11.Debug.ID3D11InfoQueue>();
            if (debug == null || infoQueue == null)
            {
                ComDiagnostics.Write("D3D11 live objects", ["  (debug layer is not available)"]);
                debug?.Dispose();
                infoQueue?.Dispose();
                return;
            }
            using (debug)
            using (infoQueue)
            {
                infoQueue.ClearStoredMessages();
                debug.ReportLiveDeviceObjects(Vortice.Direct3D11.Debug.ReportLiveDeviceObjectFlags.Detail | Vortice.Direct3D11.Debug.ReportLiveDeviceObjectFlags.IgnoreInternal);
                var lines = new List<string>();
                for (ulong i = 0; i < infoQueue.NumStoredMessages; i++)
                {
                    lines.Add("  " + infoQueue.GetMessage(i).Description);
                }
                ComDiagnostics.Write("D3D11 live objects", [.. lines]);
            }
        }
    }
}
